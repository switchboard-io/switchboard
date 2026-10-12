# @switchboard/sdk

**Local-evaluation feature-flag SDK for JavaScript/TypeScript** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```js
import { connect } from "@switchboard/sdk";

const sb = await connect(sdkKey);
const on = sb.getBool("checkout-v2", { key: "user-42" }, false);
```
