# switchboard-sdk

**Local-evaluation feature-flag SDK for Python** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```python
from switchboard_sdk import connect

sb = connect(sdk_key)
on = sb.get_bool("checkout-v2", {"key": "user-42"}, default=False)
```
