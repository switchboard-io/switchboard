# @switchboard/sdk

**Local-evaluation feature-flag SDK for JavaScript/TypeScript** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```js
import { SwitchboardClient } from "@switchboard/sdk";

// `snapshot` is the flag-config JSON { "flags": [ ... ] }, e.g. from the server's /api/flags
const client = SwitchboardClient.fromJson(snapshot);

const ctx = { key: "user-42", attributes: { country: "US" } };
const on = client.getBool("checkout-v2", ctx, false);

// full detail: { value, variationIndex, reason }
const detail = client.evaluateDetail("checkout-v2", ctx);
```
