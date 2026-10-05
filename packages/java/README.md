# Switchboard SDK (Java)

**Local-evaluation feature-flag SDK for Java** — real-time streaming updates, offline-capable, and [OpenFeature](https://openfeature.dev)-compatible.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install (Maven)

```xml
<dependency>
  <groupId>io.github.switchboard-io</groupId>
  <artifactId>sdk</artifactId>
  <version>0.0.1</version>
</dependency>
```

## Planned usage

```java
var sb = Switchboard.connect(sdkKey);
boolean on = sb.getBool("checkout-v2", Context.of("key", "user-42"), false);
```
