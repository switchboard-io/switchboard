# switchboard-sdk

**Local-evaluation feature-flag SDK for Python** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `switchboard-sdk` name on PyPI while the full SDK is built.

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
