# switchboard-openfeature (Ruby)

**OpenFeature provider for Switchboard (Ruby)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
gem install switchboard-openfeature
```

## Planned usage

```ruby
require "switchboard/openfeature"

OpenFeature::SDK.configure { |c| c.set_provider(Switchboard::OpenFeature::Provider.new(sdk_key)) }
client = OpenFeature::SDK.build_client
on = client.fetch_boolean_value(flag_key: "checkout-v2", default_value: false)
```
