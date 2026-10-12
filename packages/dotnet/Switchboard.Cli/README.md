# Switchboard.Cli

**The `switchboard` command-line tool** for [Switchboard](https://switchboard.co) feature management — create flags, toggle them, run progressive rollouts, evaluate contexts, and manage flag lifecycle from your terminal or CI.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Commands

Built from source (`dotnet run --project packages/dotnet/Switchboard.Cli -- …`):

```bash
# Evaluate a flag against a context using the real engine
switchboard eval checkout-v2 --config flags.json --context '{"key":"user-42","attributes":{"country":"US"}}'

# Print the deterministic rollout bucket in [0,1) for a key
switchboard bucket checkout-v2 abc user-1
```
