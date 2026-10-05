# switchboard-sdk (Ruby)

**Local-evaluation feature-flag SDK for Ruby** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `switchboard-sdk` gem name on RubyGems while the full SDK is built.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
gem install switchboard-sdk
```

## Planned usage

```ruby
require "switchboard/sdk"

sb = Switchboard::Sdk.connect(sdk_key)
on = sb.get_bool("checkout-v2", { key: "user-42" }, default: false)
```
