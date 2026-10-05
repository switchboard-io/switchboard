# Guide

A tour of Switchboard's concepts. The authoritative behavior is in [SPEC.md](SPEC.md).

## Flags

A **flag** has a key, a list of **variations** (the possible return values — booleans,
strings, numbers, or JSON), and per-environment configuration describing which
variation a given context should receive.

```json
{
  "key": "checkout-v2",
  "enabled": true,
  "variations": ["off", "on"],
  "offVariation": 0,
  "fallthrough": { "variation": 0 },
  "salt": "abc"
}
```

## Evaluation context

You evaluate a flag **for a context** — who or what you're deciding about:

```json
{ "key": "user-42", "attributes": { "country": "US", "plan": "pro", "age": 30 } }
```

The `key` identifies the subject (used for targeting and stable rollout bucketing);
`attributes` feed targeting rules.

## Evaluation order

Switchboard evaluates in a fixed order and tells you **why** via a `reason`:

1. **OFF** — the flag is disabled → returns `offVariation`.
2. **PREREQUISITE_FAILED** — a required prerequisite flag didn't return its expected
   variation.
3. **TARGET_MATCH** — the context key is individually targeted.
4. **RULE_MATCH** — a targeting rule matched (all its clauses are true).
5. **FALLTHROUGH** — nothing else matched; the default applies.

## Targeting rules

A rule is a list of **clauses** (ANDed together) plus the variation to serve. Clauses
support `in`, `contains`, `startsWith`, `endsWith`, `greaterThan`, `lessThan`, and
`regexMatch`, each optionally negated.

```json
{ "clauses": [
    { "attribute": "country", "op": "in", "values": ["US", "CA"] },
    { "attribute": "age", "op": "greaterThan", "values": [18] }
  ], "variation": 1 }
```

## Progressive rollouts

Instead of a single variation, a rule or fallthrough can **roll out** by percentage:

```json
{ "rollout": [ { "variation": 0, "weight": 75000 }, { "variation": 1, "weight": 25000 } ] }
```

Weights are out of 100000 (so this is 75% / 25%). The slice a context lands in is
determined by a **deterministic SHA1 bucket** of `flagKey.salt.contextKey` — so the
same user always lands in the same slice, and **every language computes the identical
bucket** (proven in [conformance](../conformance/RESULTS.md)). Ramp a release by
raising the weight over time; users already in the slice stay stable.

## Prerequisites

A flag can depend on another flag returning a specific variation — useful for gating a
feature behind a parent toggle:

```json
"prerequisites": [ { "key": "beta-program", "variation": 1 } ]
```

## Kill switch

Set `enabled: false` (or flip the fallthrough to the off variation) to instantly turn a
feature off everywhere. With the streaming server, SDKs pick up the change in
milliseconds; offline, they fall back to their last-known-good snapshot.

## OpenFeature

Every language ships an [OpenFeature](https://openfeature.dev) provider, so you can use
the vendor-neutral OpenFeature API and swap Switchboard in without rewriting call sites.
