# switchboard-sdk (Ruby)

**Local-evaluation feature-flag SDK for Ruby** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```ruby
require "switchboard/sdk"

# `snapshot` is the flag-config { "flags" => [ ... ] } (hash or JSON string), e.g. from /api/flags
client = Switchboard::Sdk::Client.from_json(snapshot)

ctx = { "key" => "user-42", "attributes" => { "country" => "US" } }
r = client.evaluate_detail("checkout-v2", ctx)
# r["value"], r["variationIndex"], r["reason"]
```
