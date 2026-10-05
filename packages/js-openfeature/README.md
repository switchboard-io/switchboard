# @switchboard/openfeature-provider

**OpenFeature provider for Switchboard (JS/TS)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `@switchboard/openfeature-provider` name on npm while the full provider is built.

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
