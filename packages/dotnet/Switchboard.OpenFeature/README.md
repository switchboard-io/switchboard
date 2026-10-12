# Switchboard.OpenFeature

**OpenFeature provider for Switchboard (.NET)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
dotnet add package Switchboard.OpenFeature
```

## Planned usage

```csharp
using OpenFeature;
using Switchboard.OpenFeature;

await Api.Instance.SetProviderAsync(new SwitchboardProvider(sdkKey));
var client = Api.Instance.GetClient();
bool on = await client.GetBooleanValueAsync("checkout-v2", false);
```
