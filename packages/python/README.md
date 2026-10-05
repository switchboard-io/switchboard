# switchboard-sdk

**Local-evaluation feature-flag SDK for Python** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
pip install switchboard-sdk
```

## Planned usage

```python
from switchboard_sdk import connect

sb = connect(sdk_key)
on = sb.get_bool("checkout-v2", {"key": "user-42"}, default=False)
```
