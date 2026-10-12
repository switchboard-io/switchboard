# Switchboard SDK (Java)

**Local-evaluation feature-flag SDK for Java** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Build from source.** This SDK is complete and conformance-tested; build it from the [monorepo](https://github.com/switchboard-io/switchboard).

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Usage

```java
import io.github.switchboard.sdk.Switchboard;
import java.util.Map;

// `snapshot` is the flag-config JSON { "flags": [ ... ] }, e.g. from the server's /api/flags
Switchboard.Client client = Switchboard.Client.fromJson(snapshot);

Map<String, Object> ctx = Map.of("key", "user-42", "attributes", Map.of("country", "US"));
Switchboard.Result r = client.evaluateDetail("checkout-v2", ctx);
// r.value, r.variationIndex, r.reason
```
