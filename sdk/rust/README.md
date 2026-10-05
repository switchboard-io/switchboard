# switchboard-sdk (Rust)

**Local-evaluation feature-flag SDK for Rust** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `switchboard-sdk` crate name on crates.io while the full SDK is built.

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
