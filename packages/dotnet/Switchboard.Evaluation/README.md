# Switchboard.Evaluation

The deterministic **feature-flag evaluation engine** shared by the Switchboard server
and every .NET SDK. Pure and side-effect-free: given a flag config and a context it
returns a value, variation index, and reason.

Implements the [Switchboard evaluation spec](https://github.com/switchboard-io/switchboard/blob/main/docs/SPEC.md):
targeting, segment rules, prerequisites, and **SHA1-based deterministic rollout bucketing**
that is byte-for-byte identical across all six language SDKs.

- Website: https://switchboard.co
- License: Apache-2.0

```csharp
var flag = FlagConfig.Parse(json);
var result = Evaluator.Evaluate(flag, new EvalContext("user-42").Set("country", JsonValue.Create("US")));
// result.Value, result.VariationIndex, result.Reason
```