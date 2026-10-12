# switchboard-openfeature-provider

**OpenFeature provider for Switchboard (Python)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```python
from switchboard_sdk import SwitchboardClient
from switchboard_openfeature_provider import SwitchboardProvider

client = SwitchboardClient.from_json(snapshot)
provider = SwitchboardProvider(client)

ctx = {"key": "user-42", "attributes": {"country": "US"}}
result = provider.resolve_boolean_details("checkout-v2", False, ctx)  # {"value", "reason"}
```
