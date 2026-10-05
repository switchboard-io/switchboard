# Switchboard.OpenFeature

**OpenFeature provider for Switchboard (.NET)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `Switchboard.OpenFeature` ID on NuGet.org while the full provider is built.

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
