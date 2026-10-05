package io.github.switchboard.sdk;

import java.util.Map;

/** Dependency-free self-test runnable with `java -ea`. Mirrors the other SDKs' tests. */
public final class SelfTest {

    static int passed = 0;

    static void check(String name, boolean cond) {
        if (!cond) throw new AssertionError("FAILED: " + name);
        passed++;
        System.out.println("  ok  " + name);
    }

    @SuppressWarnings("unchecked")
    static Map<String, Object> ctx(String json) {
        return (Map<String, Object>) MiniJson.parse(json);
    }

    public static void main(String[] args) {
        // Bucket parity with the shared reference vectors.
        check("bucket f.s.user-A", Switchboard.bucketOf("f", "s", "user-A") == 0.250274217889144);
        check("bucket checkout-v2.abc.user-1", Switchboard.bucketOf("checkout-v2", "abc", "user-1") == 0.5864417850195552);

        String snapshot = "{ \"flags\": ["
            + "{ \"key\":\"disabled\", \"enabled\":false, \"variations\":[false,true], \"offVariation\":0, \"fallthrough\":{\"variation\":1}, \"salt\":\"s\" },"
            + "{ \"key\":\"target\", \"enabled\":true, \"variations\":[false,true], \"offVariation\":0, \"targets\":[{\"values\":[\"vip\"],\"variation\":1}], \"fallthrough\":{\"variation\":0}, \"salt\":\"s\" },"
            + "{ \"key\":\"rule\", \"enabled\":true, \"variations\":[\"off\",\"on\"], \"offVariation\":0, \"rules\":[{\"clauses\":[{\"attribute\":\"country\",\"op\":\"in\",\"values\":[\"US\",\"CA\"]}],\"variation\":1}], \"fallthrough\":{\"variation\":0}, \"salt\":\"s\" },"
            + "{ \"key\":\"rollout\", \"enabled\":true, \"variations\":[false,true], \"offVariation\":0, \"fallthrough\":{\"rollout\":[{\"variation\":0,\"weight\":50000},{\"variation\":1,\"weight\":50000}]}, \"salt\":\"s\" }"
            + "] }";

        Switchboard.Client c = Switchboard.Client.fromJson(snapshot);

        Switchboard.Result r;
        r = c.evaluateDetail("disabled", ctx("{\"key\":\"u\"}"));
        check("disabled -> OFF", r.reason.equals("OFF") && Boolean.FALSE.equals(r.value));

        r = c.evaluateDetail("target", ctx("{\"key\":\"vip\"}"));
        check("target match", r.reason.equals("TARGET_MATCH") && Boolean.TRUE.equals(r.value));

        r = c.evaluateDetail("rule", ctx("{\"key\":\"u\",\"attributes\":{\"country\":\"US\"}}"));
        check("rule match (US)", r.reason.equals("RULE_MATCH") && "on".equals(r.value));

        r = c.evaluateDetail("rule", ctx("{\"key\":\"u\",\"attributes\":{\"country\":\"IN\"}}"));
        check("fallthrough (IN)", r.reason.equals("FALLTHROUGH"));

        int a = c.evaluateDetail("rollout", ctx("{\"key\":\"user-A\"}")).variationIndex;
        int b = c.evaluateDetail("rollout", ctx("{\"key\":\"user-A\"}")).variationIndex;
        check("rollout deterministic", a == b);

        System.out.println("\n" + passed + " passed");
    }
}
