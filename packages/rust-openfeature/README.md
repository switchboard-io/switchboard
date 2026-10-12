# switchboard-openfeature (Rust)

**OpenFeature provider for Switchboard (Rust)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```rust
let provider = switchboard_openfeature::SwitchboardProvider::new(sdk_key);
open_feature::OpenFeature::singleton_mut().await.set_provider(provider).await;
```
