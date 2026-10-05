# Switchboard OpenFeature Provider (Java)

**OpenFeature provider for Switchboard (Java)** — plug Switchboard into any [OpenFeature](https://openfeature.dev)-based application.

> **Not yet published.** The SDK is complete and conformance-tested — for now, build it from source from the [monorepo](https://github.com/switchboard-io/switchboard). Pre-built packages are coming.

- Website: https://switchboard.co
- Source: https://github.com/switchboard-io/switchboard
- License: Apache-2.0

## Install (Maven)

```xml
<dependency>
  <groupId>io.github.switchboard-io</groupId>
  <artifactId>openfeature-provider</artifactId>
  <version>0.0.1</version>
</dependency>
```

## Planned usage

```java
OpenFeatureAPI.getInstance().setProvider(new SwitchboardProvider(sdkKey));
var client = OpenFeatureAPI.getInstance().getClient();
boolean on = client.getBooleanValue("checkout-v2", false);
```
