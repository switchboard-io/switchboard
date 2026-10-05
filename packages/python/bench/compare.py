#!/usr/bin/env python3
"""Benchmark: evaluations/sec for switchboard-sdk."""
import time

from switchboard_sdk import SwitchboardClient

client = SwitchboardClient.from_json({"flags": [{
    "key": "checkout-v2", "enabled": True, "variations": ["off", "on"], "offVariation": 0,
    "targets": [{"values": ["vip"], "variation": 1}],
    "rules": [{"clauses": [{"attribute": "country", "op": "in", "values": ["US", "CA", "GB"]}], "variation": 1}],
    "fallthrough": {"rollout": [{"variation": 0, "weight": 75000}, {"variation": 1, "weight": 25000}]},
    "salt": "abc",
}]})


def ctx(i):
    return {"key": f"user-{i}", "attributes": {"country": "US" if i % 2 == 0 else "IN"}}


for i in range(50_000):  # warmup
    client.evaluate_detail("checkout-v2", ctx(i))

N = 500_000
start = time.perf_counter()
sink = 0
for i in range(N):
    sink += client.evaluate_detail("checkout-v2", ctx(i))["variationIndex"]
elapsed = time.perf_counter() - start

print("Switchboard SDK benchmark (Python)")
print(f"  evaluations : {N:,}")
print(f"  total time  : {elapsed * 1000:.1f} ms")
print(f"  per eval    : {elapsed / N * 1e9:.1f} ns")
print(f"  throughput  : {int(N / elapsed):,} evals/sec")
print(f"  (sink={sink})")
