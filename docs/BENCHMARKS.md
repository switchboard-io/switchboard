# Benchmarks

Every Switchboard SDK evaluates flags **locally, in-process** — no network call per
flag check. These numbers measure raw evaluation throughput of the shared engine on a
representative flag (an individual target, a multi-value `in` rule, and a percentage
rollout with bucketing) evaluated against a fresh context each iteration.

> Reproducible in CI via `.github/workflows/benchmarks.yml` (manual + weekly). Numbers
> below were measured on a developer workstation (Windows, .NET 10 / Node 24 / CPython
> 3.14 / Go 1.25 / Rust stable) and will vary by machine — treat them as orders of
> magnitude, not guarantees.

## Evaluation throughput

| Language | per evaluation | throughput |
|----------|----------------|-----------|
| Node     | ~0.93 µs | ~1,070,000 evals/sec |
| .NET     | ~1.01 µs | ~990,000 evals/sec |
| Go       | ~3.1 µs  | ~320,000 evals/sec |
| Rust¹    | ~4.2 µs  | ~240,000 evals/sec |
| Python   | ~7.0 µs  | ~143,000 evals/sec |

Each run includes a warmup phase and a measured phase (500k–2M evaluations), with the
result accumulated into a sink to prevent dead-code elimination.

¹ The Rust client currently clones its flag map per call for prerequisite resolution;
removing that allocation (a planned optimization) brings it in line with .NET/Go. The
number above is the honest, un-optimized measurement — the point of running benchmarks
in CI is that we quote what we measure, not what we hope.

## What this means in practice

Even the slowest implementation evaluates **>100,000 flags per second per core**, and
evaluation is local so it adds no network latency to a request. For a typical service
checking a handful of flags per request, flag evaluation is effectively free — the
engineering focus is correctness and consistency (see [conformance](../conformance)),
not raw speed.

## Reproduce locally

```bash
dotnet run -c Release --project packages/dotnet/Switchboard.Bench
node packages/js/bench/compare.js
python packages/python/bench/compare.py
(cd packages/go && go run ./cmd/bench)
cargo run --release --example compare --manifest-path packages/rust/Cargo.toml
```
