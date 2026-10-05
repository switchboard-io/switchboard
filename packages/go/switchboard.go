// Package switchboard is the local-evaluation feature-flag engine for Go.
//
// It implements docs/SPEC.md identically to the other language SDKs.
package switchboard

import (
	"crypto/sha1"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"math"
	"strconv"
	"strings"
)

const maxBucket = float64(0xFFFFFFFFFFFFF) // 2^52 - 1

// BucketOf returns a deterministic rollout bucket in [0, 1) — identical across all SDKs (SPEC §4).
func BucketOf(flagKey, salt, contextKey string) float64 {
	input := fmt.Sprintf("%s.%s.%s", flagKey, salt, contextKey)
	sum := sha1.Sum([]byte(input))
	h := hex.EncodeToString(sum[:]) // 40 lowercase hex chars
	n, _ := strconv.ParseInt(h[:13], 16, 64)
	return float64(n) / maxBucket
}

// Result is the outcome of an evaluation.
type Result struct {
	Value          interface{} `json:"value"`
	VariationIndex int         `json:"variationIndex"`
	Reason         string      `json:"reason"`
}

func errResult() Result { return Result{Value: nil, VariationIndex: -1, Reason: "ERROR"} }

// Resolver returns a prerequisite flag by key, or nil.
type Resolver func(key string) map[string]interface{}

func asNumber(v interface{}) (float64, bool) {
	switch n := v.(type) {
	case float64:
		return n, true
	case int:
		return float64(n), true
	case int64:
		return float64(n), true
	}
	return 0, false
}

func asString(v interface{}) (string, bool) {
	switch s := v.(type) {
	case string:
		return s, true
	case bool:
		if s {
			return "true", true
		}
		return "false", true
	case float64:
		if s == math.Trunc(s) {
			return strconv.FormatInt(int64(s), 10), true
		}
		return strconv.FormatFloat(s, 'g', -1, 64), true
	}
	return "", false
}

func scalarEquals(a, b interface{}) bool {
	an, aok := asNumber(a)
	bn, bok := asNumber(b)
	if aok && bok {
		return an == bn
	}
	ab, aIsBool := a.(bool)
	bb, bIsBool := b.(bool)
	if aIsBool && bIsBool {
		return ab == bb
	}
	as, _ := asString(a)
	bs, _ := asString(b)
	return as == bs
}

func toSlice(v interface{}) []interface{} {
	if s, ok := v.([]interface{}); ok {
		return s
	}
	return nil
}

func matchOp(op string, attr interface{}, values []interface{}) bool {
	if attr == nil {
		return false
	}
	switch op {
	case "in":
		for _, v := range values {
			if scalarEquals(attr, v) {
				return true
			}
		}
		return false
	case "contains", "startsWith", "endsWith":
		s, ok := asString(attr)
		if !ok {
			return false
		}
		for _, v := range values {
			t, ok := asString(v)
			if !ok {
				continue
			}
			switch op {
			case "contains":
				if strings.Contains(s, t) {
					return true
				}
			case "startsWith":
				if strings.HasPrefix(s, t) {
					return true
				}
			case "endsWith":
				if strings.HasSuffix(s, t) {
					return true
				}
			}
		}
		return false
	case "greaterThan", "lessThan":
		a, ok1 := asNumber(attr)
		if !ok1 || len(values) == 0 {
			return false
		}
		b, ok2 := asNumber(values[0])
		if !ok2 {
			return false
		}
		if op == "greaterThan" {
			return a > b
		}
		return a < b
	case "regexMatch":
		// Substring fallback to avoid importing regexp semantics that differ per language.
		s, ok := asString(attr)
		if !ok || len(values) == 0 {
			return false
		}
		p, ok := asString(values[0])
		return ok && strings.Contains(s, p)
	}
	return false
}

func matchClause(clause, ctx map[string]interface{}) bool {
	attribute, _ := clause["attribute"].(string)
	var attr interface{}
	if attribute == "key" {
		attr = ctx["key"]
	} else if attrs, ok := ctx["attributes"].(map[string]interface{}); ok {
		attr = attrs[attribute]
	}
	op, _ := clause["op"].(string)
	r := matchOp(op, attr, toSlice(clause["values"]))
	if neg, _ := clause["negate"].(bool); neg {
		return !r
	}
	return r
}

func toInt(v interface{}) (int, bool) {
	if f, ok := v.(float64); ok {
		return int(f), true
	}
	return 0, false
}

func resolveIndex(flag map[string]interface{}, variation interface{}, rollout []interface{}, ctx map[string]interface{}) (int, bool) {
	if idx, ok := toInt(variation); ok {
		return idx, true
	}
	if len(rollout) > 0 {
		flagKey, _ := flag["key"].(string)
		salt, _ := flag["salt"].(string)
		ctxKey, _ := ctx["key"].(string)
		bucket := BucketOf(flagKey, salt, ctxKey)
		cumulative := 0.0
		for _, item := range rollout {
			wv, _ := item.(map[string]interface{})
			w, _ := asNumber(wv["weight"])
			cumulative += w
			if bucket*100000 < cumulative {
				idx, _ := toInt(wv["variation"])
				return idx, true
			}
		}
		last, _ := rollout[len(rollout)-1].(map[string]interface{})
		idx, _ := toInt(last["variation"])
		return idx, true
	}
	return 0, false
}

func makeResult(flag map[string]interface{}, index int, ok bool, reason string) Result {
	if !ok {
		return errResult()
	}
	variations := toSlice(flag["variations"])
	if index < 0 || index >= len(variations) {
		return errResult()
	}
	return Result{Value: variations[index], VariationIndex: index, Reason: reason}
}

// Evaluate a flag for a context. resolver returns prerequisite flags by key.
func Evaluate(flag, ctx map[string]interface{}, resolver Resolver) Result {
	off, _ := toInt(flag["offVariation"])

	if enabled, _ := flag["enabled"].(bool); !enabled {
		return makeResult(flag, off, true, "OFF")
	}

	for _, p := range toSlice(flag["prerequisites"]) {
		pm, _ := p.(map[string]interface{})
		key, _ := pm["key"].(string)
		var pre map[string]interface{}
		if resolver != nil {
			pre = resolver(key)
		}
		want, _ := toInt(pm["variation"])
		if pre == nil || Evaluate(pre, ctx, resolver).VariationIndex != want {
			return makeResult(flag, off, true, "PREREQUISITE_FAILED")
		}
	}

	ctxKey, _ := ctx["key"].(string)
	for _, t := range toSlice(flag["targets"]) {
		tm, _ := t.(map[string]interface{})
		for _, v := range toSlice(tm["values"]) {
			if s, ok := v.(string); ok && s == ctxKey {
				idx, _ := toInt(tm["variation"])
				return makeResult(flag, idx, true, "TARGET_MATCH")
			}
		}
	}

	for _, r := range toSlice(flag["rules"]) {
		rule, _ := r.(map[string]interface{})
		clauses := toSlice(rule["clauses"])
		all := true
		for _, c := range clauses {
			cm, _ := c.(map[string]interface{})
			if !matchClause(cm, ctx) {
				all = false
				break
			}
		}
		if all {
			idx, ok := resolveIndex(flag, rule["variation"], toSlice(rule["rollout"]), ctx)
			return makeResult(flag, idx, ok, "RULE_MATCH")
		}
	}

	ft, _ := flag["fallthrough"].(map[string]interface{})
	if ft == nil {
		ft = map[string]interface{}{}
	}
	idx, ok := resolveIndex(flag, ft["variation"], toSlice(ft["rollout"]), ctx)
	return makeResult(flag, idx, ok, "FALLTHROUGH")
}

// Client holds a snapshot of flags and evaluates them locally.
type Client struct {
	flags map[string]map[string]interface{}
}

// FromJSON builds a Client from a snapshot: { "flags": [ ...FlagConfig ] }.
func FromJSON(data string) *Client {
	var parsed map[string]interface{}
	_ = json.Unmarshal([]byte(data), &parsed)
	flags := map[string]map[string]interface{}{}
	for _, f := range toSlice(parsed["flags"]) {
		fm, _ := f.(map[string]interface{})
		if key, ok := fm["key"].(string); ok {
			flags[key] = fm
		}
	}
	return &Client{flags: flags}
}

// EvaluateDetail evaluates a flag by key for the given context.
func (c *Client) EvaluateDetail(key string, ctx map[string]interface{}) Result {
	flag, ok := c.flags[key]
	if !ok {
		return errResult()
	}
	return Evaluate(flag, ctx, func(k string) map[string]interface{} { return c.flags[k] })
}

// About returns a short description of the SDK.
func About() string {
	return "switchboard-go — local-evaluation feature flags for Go. See https://switchboard.co"
}
