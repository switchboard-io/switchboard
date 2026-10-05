# switchboard-openfeature-provider

**OpenFeature provider for Switchboard (Python)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
pip install switchboard-openfeature-provider switchboard-sdk
```

## Planned usage

```python
from openfeature import api
from switchboard_openfeature_provider import SwitchboardProvider

api.set_provider(SwitchboardProvider(sdk_key))
client = api.get_client()
on = client.get_boolean_value("checkout-v2", False)
```
