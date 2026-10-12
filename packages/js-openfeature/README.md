# @switchboard/openfeature-provider

**OpenFeature provider for Switchboard (JS/TS)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```js
import { OpenFeature } from "@openfeature/server-sdk";
import { SwitchboardProvider } from "@switchboard/openfeature-provider";

await OpenFeature.setProviderAndWait(new SwitchboardProvider(sdkKey));
const client = OpenFeature.getClient();
const on = await client.getBooleanValue("checkout-v2", false);
```
