# switchboard-go

**Local-evaluation feature-flag SDK for Go** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
go get github.com/switchboard-io/switchboard-go
```

## Planned usage

```go
import switchboard "github.com/switchboard-io/switchboard-go"

sb, _ := switchboard.Connect(sdkKey)
on := sb.GetBool("checkout-v2", switchboard.Context{"key": "user-42"}, false)
```
