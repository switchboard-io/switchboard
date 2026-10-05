# switchboard-sdk (Rust)

**Local-evaluation feature-flag SDK for Rust** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
cargo add switchboard-sdk
```

## Planned usage

```rust
let sb = switchboard_sdk::connect(sdk_key).await?;
let on = sb.get_bool("checkout-v2", &ctx, false);
```
