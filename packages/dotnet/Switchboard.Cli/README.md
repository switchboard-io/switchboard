# Switchboard.Cli

**The `switchboard` command-line tool** for [Switchboard](https://switchboard.co) feature management — create flags, toggle them, run progressive rollouts, evaluate contexts, and manage flag lifecycle from your terminal or CI.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
dotnet tool install -g Switchboard.Cli
switchboard --help
```

## Planned commands

```bash
switchboard login --url https://switchboard.mycorp.internal
switchboard flag create checkout-v2 --type temporary
switchboard flag rollout checkout-v2 --env production --percent 25
switchboard eval checkout-v2 --context '{"key":"user-42","plan":"pro"}'
switchboard export --project web > flags.yaml
switchboard lifecycle stale --days 30
```
