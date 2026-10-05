# Switchboard.Sdk

**Local-evaluation feature-flag SDK for .NET** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

## What Switchboard is

Switchboard is a self-hosted, open feature-management platform: feature flags, targeting, progressive rollouts, kill switches, and experimentation — with no vendor lock-in and built-in flag-lifecycle management.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
dotnet add package Switchboard.Sdk
```

## Planned usage

```csharp
using Switchboard;

var about = SwitchboardClient.About(); // placeholder API
// Full API (coming soon):
//   var sb = await Switchboard.ConnectAsync(sdkKey);
//   bool on = sb.GetBool("checkout-v2", context, defaultValue: false);
```
