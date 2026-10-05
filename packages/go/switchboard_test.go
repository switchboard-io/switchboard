package switchboard

import (
	"encoding/json"
	"testing"
)

const snapshot = `{ "flags": [
  { "key":"disabled", "enabled":false, "variations":[false,true], "offVariation":0, "fallthrough":{"variation":1}, "salt":"s" },
  { "key":"target", "enabled":true, "variations":[false,true], "offVariation":0, "targets":[{"values":["vip"],"variation":1}], "fallthrough":{"variation":0}, "salt":"s" },
  { "key":"rule", "enabled":true, "variations":["off","on"], "offVariation":0, "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA"]}],"variation":1}], "fallthrough":{"variation":0}, "salt":"s" },
  { "key":"rollout", "enabled":true, "variations":[false,true], "offVariation":0, "fallthrough":{"rollout":[{"variation":0,"weight":50000},{"variation":1,"weight":50000}]}, "salt":"s" }
] }`

func ctx(j string) map[string]interface{} {
	var m map[string]interface{}
	_ = json.Unmarshal([]byte(j), &m)
	return m
}

func TestBucketParity(t *testing.T) {
	if got := BucketOf("f", "s", "user-A"); got != 0.250274217889144 {
		t.Fatalf("bucket f.s.user-A = %v, want 0.250274217889144", got)
	}
	if got := BucketOf("checkout-v2", "abc", "user-1"); got != 0.5864417850195552 {
		t.Fatalf("bucket checkout-v2.abc.user-1 = %v, want 0.5864417850195552", got)
	}
}

func TestEvaluation(t *testing.T) {
	c := FromJSON(snapshot)

	if r := c.EvaluateDetail("disabled", ctx(`{"key":"u"}`)); r.Reason != "OFF" || r.Value != false {
		t.Fatalf("disabled: %+v", r)
	}
	if r := c.EvaluateDetail("target", ctx(`{"key":"vip"}`)); r.Reason != "TARGET_MATCH" || r.Value != true {
		t.Fatalf("target: %+v", r)
	}
	if r := c.EvaluateDetail("rule", ctx(`{"key":"u","attributes":{"country":"US"}}`)); r.Reason != "RULE_MATCH" || r.Value != "on" {
		t.Fatalf("rule: %+v", r)
	}
	if r := c.EvaluateDetail("rule", ctx(`{"key":"u","attributes":{"country":"IN"}}`)); r.Reason != "FALLTHROUGH" {
		t.Fatalf("fallthrough: %+v", r)
	}

	a := c.EvaluateDetail("rollout", ctx(`{"key":"user-A"}`)).VariationIndex
	b := c.EvaluateDetail("rollout", ctx(`{"key":"user-A"}`)).VariationIndex
	if a != b {
		t.Fatalf("rollout not deterministic: %d vs %d", a, b)
	}
}
