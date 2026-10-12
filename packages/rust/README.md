# switchboard-sdk (Rust)

**Local-evaluation feature-flag SDK for Rust** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```rust
use serde_json::json;
use switchboard_sdk::SwitchboardClient;

// `snapshot` is the flag-config JSON string { "flags": [ ... ] }, e.g. from /api/flags
let client = SwitchboardClient::from_json(snapshot);

let ctx = json!({ "key": "user-42", "attributes": { "country": "US" } });
let r = client.evaluate_detail("checkout-v2", &ctx);
// r.value, r.variation_index, r.reason
```
