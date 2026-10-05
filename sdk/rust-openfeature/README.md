# switchboard-openfeature (Rust)

**OpenFeature provider for Switchboard (Rust)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `switchboard-openfeature` crate name on crates.io while the full provider is built.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
cargo add switchboard-openfeature
```

## Planned usage

```rust
let provider = switchboard_openfeature::SwitchboardProvider::new(sdk_key);
open_feature::OpenFeature::singleton_mut().await.set_provider(provider).await;
```
