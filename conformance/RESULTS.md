# Conformance results

Every Switchboard SDK evaluates the shared corpus in `corpus.json`. This matrix,
regenerated in CI on every push, proves they all agree with the expected result
defined by [`docs/SPEC.md`](../docs/SPEC.md).

**Status: 175/175 checks passing across 7 languages — ALL PASSING.**

## Evaluation cases

| Case | Expected | dotnet | node | python | go | java | rust | ruby |
|------|----------|----|----|----|----|----|----|----|
| `disabled` | `OFF` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `target-hit` | `TARGET_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `target-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `in-hit` | `RULE_MATCH` / `"on"` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `in-miss` | `FALLTHROUGH` / `"off"` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `gt-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `gt-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `lt-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `lt-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `contains-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `contains-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `starts-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `starts-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `ends-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `ends-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `negate-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `negate-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `multi-hit` | `RULE_MATCH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `multi-miss` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `rollout-A` | `FALLTHROUGH` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `rollout-C` | `FALLTHROUGH` / `true` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `prereq-fail` | `PREREQUISITE_FAILED` / `false` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

## Rollout bucket vectors (exact cross-language identity)

| Vector | Expected | dotnet | node | python | go | java | rust | ruby |
|--------|----------|----|----|----|----|----|----|----|
| `f.s.user-A` | `0.250274217889144` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `checkout-v2.abc.user-1` | `0.5864417850195552` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `rollout.s.user-C` | `0.5457655356360461` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
