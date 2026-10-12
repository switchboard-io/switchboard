# Switchboard.OpenFeature

**OpenFeature provider for Switchboard (.NET)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```csharp
using Switchboard.Evaluation;
using Switchboard.OpenFeature;
using System.Text.Json.Nodes;

// `snapshot` is the flag-config JSON { "flags": [ ... ] }, e.g. from the server's /api/flags
var provider = SwitchboardProvider.FromJson(snapshot);

var ctx = new EvalContext("user-42").Set("country", JsonValue.Create("US"));
var (on, reason) = provider.ResolveBoolean("checkout-v2", ctx, def: false);
```
