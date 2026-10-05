# switchboard-openfeature (Rust)

**OpenFeature provider for Switchboard (Rust)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

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
