// Minimal dependency-free test runner for @switchboard/sdk (ESM).
import assert from "assert";
import { bucketOf, SwitchboardClient } from "../index.js";

let pass = 0;
function test(name, fn) { fn(); pass++; console.log("  ok  " + name); }

test("bucket parity with reference vector", () => {
  assert.strictEqual(bucketOf("f", "s", "user-A"), 0.250274217889144);
  assert.strictEqual(bucketOf("checkout-v2", "abc", "user-1"), 0.5864417850195552);
});

const client = SwitchboardClient.fromJson({ flags: [
  { key: "disabled", enabled: false, variations: [false, true], offVariation: 0, fallthrough: { variation: 1 }, salt: "s" },
  { key: "target", enabled: true, variations: [false, true], offVariation: 0, targets: [{ values: ["vip"], variation: 1 }], fallthrough: { variation: 0 }, salt: "s" },
  { key: "rule", enabled: true, variations: ["off", "on"], offVariation: 0, rules: [{ clauses: [{ attribute: "country", op: "in", values: ["US", "CA"] }], variation: 1 }], fallthrough: { variation: 0 }, salt: "s" },
  { key: "rollout", enabled: true, variations: [false, true], offVariation: 0, fallthrough: { rollout: [{ variation: 0, weight: 50000 }, { variation: 1, weight: 50000 }] }, salt: "s" },
]});

test("disabled -> OFF", () => { const r = client.evaluateDetail("disabled", { key: "u" }); assert.strictEqual(r.reason, "OFF"); assert.strictEqual(r.value, false); });
test("target match", () => { const r = client.evaluateDetail("target", { key: "vip" }); assert.strictEqual(r.reason, "TARGET_MATCH"); assert.strictEqual(r.value, true); });
test("rule match (US)", () => { const r = client.evaluateDetail("rule", { key: "u", attributes: { country: "US" } }); assert.strictEqual(r.reason, "RULE_MATCH"); assert.strictEqual(r.value, "on"); });
test("fallthrough (IN)", () => { const r = client.evaluateDetail("rule", { key: "u", attributes: { country: "IN" } }); assert.strictEqual(r.reason, "FALLTHROUGH"); });
test("rollout deterministic", () => { const a = client.evaluateDetail("rollout", { key: "user-A" }).variationIndex; const b = client.evaluateDetail("rollout", { key: "user-A" }).variationIndex; assert.strictEqual(a, b); });

console.log(`\n${pass} passed`);