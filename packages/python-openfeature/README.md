# switchboard-openfeature-provider

**OpenFeature provider for Switchboard (Python)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```python
from openfeature import api
from switchboard_openfeature_provider import SwitchboardProvider

api.set_provider(SwitchboardProvider(sdk_key))
client = api.get_client()
on = client.get_boolean_value("checkout-v2", False)
```
