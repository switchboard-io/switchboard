# Switchboard.Sdk

**Local-evaluation feature-flag SDK for .NET** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

## What Switchboard is

Switchboard is a self-hosted, open feature-management platform: feature flags, targeting, progressive rollouts, kill switches, and experimentation — with no vendor lock-in and built-in flag-lifecycle management.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```csharp
using Switchboard;
using Switchboard.Evaluation;
using System.Text.Json.Nodes;

// `snapshot` is the flag-config JSON { "flags": [ ... ] }, e.g. from the server's /api/flags
var client = SwitchboardClient.FromJson(snapshot);

var ctx = new EvalContext("user-42").Set("country", JsonValue.Create("US"));
bool on = client.GetBool("checkout-v2", ctx, defaultValue: false);

EvalResult detail = client.EvaluateDetail("checkout-v2", ctx); // Value, VariationIndex, Reason
```
