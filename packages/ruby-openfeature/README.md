# switchboard-openfeature (Ruby)

**OpenFeature provider for Switchboard (Ruby)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```ruby
require "switchboard/openfeature"

OpenFeature::SDK.configure { |c| c.set_provider(Switchboard::OpenFeature::Provider.new(sdk_key)) }
client = OpenFeature::SDK.build_client
on = client.fetch_boolean_value(flag_key: "checkout-v2", default_value: false)
```
