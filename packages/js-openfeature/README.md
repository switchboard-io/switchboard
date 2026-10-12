# @switchboard/openfeature-provider

**OpenFeature provider for Switchboard (JS/TS)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```js
import { SwitchboardClient } from "@switchboard/sdk";
import { SwitchboardProvider } from "@switchboard/openfeature-provider";

const client = SwitchboardClient.fromJson(snapshot);
const provider = new SwitchboardProvider(client);

const ctx = { key: "user-42", attributes: { country: "US" } };
const { value, reason } = provider.resolveBooleanEvaluation("checkout-v2", false, ctx);
```
