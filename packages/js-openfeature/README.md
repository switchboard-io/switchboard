# @switchboard/openfeature-provider

**OpenFeature provider for Switchboard (JS/TS)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
npm install @switchboard/openfeature-provider @switchboard/sdk
```

## Planned usage

```js
import { OpenFeature } from "@openfeature/server-sdk";
import { SwitchboardProvider } from "@switchboard/openfeature-provider";

await OpenFeature.setProviderAndWait(new SwitchboardProvider(sdkKey));
const client = OpenFeature.getClient();
const on = await client.getBooleanValue("checkout-v2", false);
```
