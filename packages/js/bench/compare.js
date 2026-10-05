// Benchmark: evaluations/sec for @switchboard/sdk.
import { SwitchboardClient } from "../index.js";

const client = SwitchboardClient.fromJson({ flags: [{
  key: "checkout-v2", enabled: true, variations: ["off", "on"], offVariation: 0,
  targets: [{ values: ["vip"], variation: 1 }],
  rules: [{ clauses: [{ attribute: "country", op: "in", values: ["US", "CA", "GB"] }], variation: 1 }],
  fallthrough: { rollout: [{ variation: 0, weight: 75000 }, { variation: 1, weight: 25000 }] },
  salt: "abc",
}]});

const ctx = (i) => ({ key: `user-${i}`, attributes: { country: i % 2 === 0 ? "US" : "IN" } });

for (let i = 0; i < 100000; i++) client.evaluateDetail("checkout-v2", ctx(i)); // warmup

const N = 2_000_000;
const start = process.hrtime.bigint();
let sink = 0;
for (let i = 0; i < N; i++) sink += client.evaluateDetail("checkout-v2", ctx(i)).variationIndex;
const ns = Number(process.hrtime.bigint() - start);

console.log("Switchboard SDK benchmark (Node)");
console.log(`  evaluations : ${N.toLocaleString()}`);
console.log(`  total time  : ${(ns / 1e6).toFixed(1)} ms`);
console.log(`  per eval    : ${(ns / N).toFixed(1)} ns`);
console.log(`  throughput  : ${Math.round(N / (ns / 1e9)).toLocaleString()} evals/sec`);
console.log(`  (sink=${sink})`);
