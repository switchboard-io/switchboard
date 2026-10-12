# switchboard-sdk

**Local-evaluation feature-flag SDK for Python** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```python
from switchboard_sdk import SwitchboardClient

# `snapshot` is the flag-config JSON {"flags": [ ... ]} (dict or string), e.g. from /api/flags
client = SwitchboardClient.from_json(snapshot)

ctx = {"key": "user-42", "attributes": {"country": "US"}}
on = client.get_bool("checkout-v2", ctx, default=False)

detail = client.evaluate_detail("checkout-v2", ctx)  # {"value", "variationIndex", "reason"}
```
