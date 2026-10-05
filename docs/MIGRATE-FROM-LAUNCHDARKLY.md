# Migrating from LaunchDarkly

Switchboard is designed to be an easy, **no-lock-in** destination for teams leaving a
hosted feature-flag vendor. This guide maps LaunchDarkly concepts to Switchboard and
walks through moving your flags, SDK calls, and targeting.

> **Why it's low-risk:** Switchboard's flag schema is plain, portable JSON; every SDK
> evaluates locally; and you self-host, so there's nothing to get locked into a second
> time. If you ever leave Switchboard, `export` hands you your whole config back.

## Concept mapping

| LaunchDarkly | Switchboard | Notes |
|--------------|-------------|-------|
| Project / Environment | Project / Environment | Same hierarchy; config is per-environment. |
| Flag (boolean/multivariate) | Flag with `variations` | `variations` holds bool / string / number / JSON values. |
| Variation | Variation (index into `variations`) | Results reference variations by index. |
| Individual targets | `targets` | `{ "values": ["user-1"], "variation": 1 }`. |
| Targeting rules + clauses | `rules` + `clauses` | Ops: `in`, `contains`, `startsWith`, `endsWith`, `greaterThan`, `lessThan`, `regexMatch` (+ `negate`). |
| Segments | Rules referencing shared clause sets | Reusable audiences (roadmap: first-class segment objects). |
| Percentage rollout | `rollout` with weights out of 100000 | Deterministic SHA1 bucketing (see `docs/SPEC.md §4`). |
| Prerequisites | `prerequisites` | `{ "key": "parent-flag", "variation": 1 }`. |
| Off variation | `offVariation` | Returned when the flag is disabled. |
| Evaluation reason | `reason` | `OFF`, `PREREQUISITE_FAILED`, `TARGET_MATCH`, `RULE_MATCH`, `FALLTHROUGH`, `ERROR`. |
| `user` / `context` | `EvalContext` (`key` + `attributes`) | Single-key context with arbitrary attributes. |
| Streaming updates | Server-Sent Events from the relay/server | SDKs cache last-known-good for offline. |

## Step 1 — Export your LaunchDarkly flags

Use LaunchDarkly's REST API to pull flag definitions per project/environment:

```bash
curl -H "Authorization: $LD_API_KEY" \
  "https://app.launchdarkly.com/api/v2/flags/<project-key>?env=<env>" > ld-flags.json
```

## Step 2 — Convert to Switchboard schema

Map each LD flag to Switchboard's shape. A boolean flag with a country rule and a 25%
rollout becomes:

```json
{
  "key": "checkout-v2",
  "enabled": true,
  "variations": [false, true],
  "offVariation": 0,
  "targets": [{ "values": ["beta-user-1"], "variation": 1 }],
  "rules": [
    { "clauses": [{ "attribute": "country", "op": "in", "values": ["US", "CA"] }], "variation": 1 }
  ],
  "fallthrough": { "rollout": [{ "variation": 0, "weight": 75000 }, { "variation": 1, "weight": 25000 }] },
  "salt": "checkout-v2"
}
```

Key differences to watch:
- **Weights are out of 100000** (LD uses a similar bucketing scale) — a 25% rollout is `weight: 25000`.
- **Variations are indexed.** `offVariation`, `targets[].variation`, and rule variations
  are integer indices into the `variations` array.
- **Bucketing isn't identical to LD's hash**, so *which* users land in the rollout slice
  will differ from LaunchDarkly. The split percentage is preserved; individual
  assignments are re-shuffled. Plan rollouts as fresh ramps rather than expecting
  per-user continuity across the migration.

## Step 3 — Load flags into Switchboard

Via the server API:

```bash
curl -X PUT http://localhost:8080/api/flags/checkout-v2 \
  -H "Content-Type: application/json" -d @checkout-v2.json
```

Or keep them as files in your repo and apply via the CLI / GitOps (flags-as-code).

## Step 4 — Swap the SDK

The call shapes are deliberately familiar. Example in .NET:

```diff
- using LaunchDarkly.Sdk;
- using LaunchDarkly.Sdk.Server;
- var client = new LdClient("sdk-key");
- var ctx = Context.Builder("user-42").Set("country", "US").Build();
- bool on = client.BoolVariation("checkout-v2", ctx, false);
+ using Switchboard;
+ using Switchboard.Evaluation;
+ var client = SwitchboardClient.FromJson(snapshotJson);  // streamed from the server
+ var ctx = new EvalContext("user-42").Set("country", JsonValue.Create("US"));
+ bool on = client.GetBool("checkout-v2", ctx, defaultValue: false);
```

Prefer **OpenFeature**? Use the Switchboard OpenFeature provider and your call sites stay
vendor-neutral — then switching costs nothing next time.

## Step 5 — Verify parity

Before cutting over, evaluate the same contexts against both systems and diff the
results. Switchboard's CLI makes spot-checks easy:

```bash
switchboard eval checkout-v2 --config flags.json \
  --context '{"key":"user-42","attributes":{"country":"US"}}'
```

Run your highest-traffic flags through a sample of real contexts and confirm the
variation distribution matches your intent (exact per-user assignment will differ due to
the different bucketing hash — see Step 2).

## Step 6 — Cut over

1. Deploy the Switchboard-wired SDK behind its own kill-switch flag.
2. Ramp traffic from LaunchDarkly to Switchboard gradually.
3. Once stable, retire the LaunchDarkly SDK and keys.

## FAQ

**Will my rollout percentages stay the same?** The *percentage* yes; the *specific users*
in each slice no (different hash). Treat migrated rollouts as new ramps.

**Can I run both during migration?** Yes — keep both SDKs initialized and gate which one
you read from behind a Switchboard flag.

**What about experiments/metrics?** Event ingestion + experimentation is on the roadmap;
for now, continue your existing analytics pipeline and use Switchboard for the flagging
and rollout control.
