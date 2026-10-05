# @switchboard/sdk

**Local-evaluation feature-flag SDK for JavaScript/TypeScript** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> ⚠️ **Placeholder release (0.0.1).** Reserves the `@switchboard/sdk` name on npm while the full SDK is built.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install

```bash
npm install @switchboard/sdk
```

## Planned usage

```js
import { connect } from "@switchboard/sdk";

const sb = await connect(sdkKey);
const on = sb.getBool("checkout-v2", { key: "user-42" }, false);
```
