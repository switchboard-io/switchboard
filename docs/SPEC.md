# Switchboard Evaluation Specification v1

This is the **single source of truth** every SDK implements identically. Conformance
(`/conformance`) verifies all six languages agree on it, case-for-case — Switchboard's
equivalent of flatwire's cross-language byte-identity proof.

## 1. Data model

### FlagConfig (JSON)
```json
{
  "key": "string",
  "enabled": true,
  "variations": [false, true],
  "offVariation": 0,
  "fallthrough": { "variation": 1 },
  "targets": [ { "values": ["user-1"], "variation": 0 } ],
  "rules": [ { "clauses": [ ... ], "variation": 1 } ],
  "prerequisites": [ { "key": "other-flag", "variation": 1 } ],
  "salt": "string"
}
```
- `variations` — ordered array of possible values (bool, string, number, or JSON).
- `offVariation` — index returned when the flag is disabled.
- `fallthrough` — `{ "variation": i }` **or** `{ "rollout": [ { "variation": i, "weight": w }, ... ] }` where weights are integers summing to 100000.
- A `rule` has `clauses` (ALL must match — logical AND) and either `variation` or `rollout`.

### EvaluationContext (JSON)
```json
{ "key": "user-42", "attributes": { "plan": "pro", "country": "US", "age": 30 } }
```

### Clause
```json
{ "attribute": "country", "op": "in", "values": ["US", "CA"], "negate": false }
```
`attribute` of `"key"` matches the context key. `negate` inverts the clause result.

## 2. Clause operators (exact set)

| op | true when |
|----|-----------|
| `in` | context value equals any of `values` (string/number/bool equality) |
| `contains` | context string contains any of `values` (substring) |
| `startsWith` | context string starts with any of `values` |
| `endsWith` | context string ends with any of `values` |
| `greaterThan` | numeric context value > `values[0]` |
| `lessThan` | numeric context value < `values[0]` |
| `regexMatch` | context string matches the regex `values[0]` (unanchored, default flags) |

A missing attribute makes the clause **false** (before `negate`). Type mismatches
(e.g. `greaterThan` on a non-number) make the clause **false** (before `negate`).

## 3. Evaluation algorithm (exact order)

```
evaluate(flag, context):
  if not flag.enabled:            return (offVariation, "OFF")
  for p in flag.prerequisites:
      pr = evaluate(prereqFlag[p.key], context)   # supplied via a resolver
      if pr.variationIndex != p.variation:
          return (offVariation, "PREREQUISITE_FAILED")
  for t in flag.targets:
      if context.key in t.values: return (t.variation, "TARGET_MATCH")
  for r in flag.rules:
      if all(matchClause(c, context) for c in r.clauses):
          return (resolve(r), "RULE_MATCH")      # rule may be variation or rollout
  return (resolve(flag.fallthrough), "FALLTHROUGH")
```

`resolve(x)` returns `x.variation` directly, or buckets when `x.rollout` is present (§4).
Any structural error (missing variation index, bad config) returns
`(default supplied by caller, "ERROR")`.

## 4. Bucketing (deterministic, cross-language identical)

```
bucketOf(flagKey, salt, contextKey) -> float in [0, 1):
    input  = flagKey + "." + salt + "." + contextKey
    digest = SHA1(input)                      # 20 bytes
    hex13  = first 13 hex chars of digest     # 52 bits
    n      = parseInt(hex13, base 16)         # <= 2^52-1, exact in IEEE-754 double
    return n / 0xFFFFFFFFFFFFF                # 4503599627370495  (2^52 - 1)
```

`resolve(rollout)`:
```
bucket = bucketOf(flag.key, flag.salt, context.key)
cumulative = 0
for wv in rollout:
    cumulative += wv.weight
    if bucket * 100000 < cumulative: return wv.variation
return rollout[last].variation
```

SHA1 is in every target language's standard library. We take **13 hex chars (52 bits)**
specifically so the integer is exactly representable as an IEEE-754 double in **every**
language — including JavaScript, whose `Number` is only safe to 2^53. That makes the
bucket — and therefore the rollout slice every user lands in — **identical across .NET,
JS, Python, Go, Java, Rust**.

## 5. Result
```json
{ "value": <the variation value>, "variationIndex": 1, "reason": "RULE_MATCH" }
```
Reasons: `OFF`, `PREREQUISITE_FAILED`, `TARGET_MATCH`, `RULE_MATCH`, `FALLTHROUGH`, `ERROR`.
