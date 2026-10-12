# switchboard-go

**Local-evaluation feature-flag SDK for Go** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```go
import switchboard "github.com/switchboard-io/switchboard-go"

// `snapshot` is the flag-config JSON { "flags": [ ... ] }, e.g. from the server's /api/flags
client := switchboard.FromJSON(snapshot)

ctx := map[string]interface{}{"key": "user-42", "attributes": map[string]interface{}{"country": "US"}}
r := client.EvaluateDetail("checkout-v2", ctx)
// r.Value, r.VariationIndex, r.Reason
```
