# Conformance

This directory is Switchboard's **cross-language proof**: every SDK must evaluate the
shared corpus identically. It's the feature-flag equivalent of a byte-identity suite.

## How it works

1. [`corpus.json`](corpus.json) — a shared set of flag configs, evaluation **cases**
   (each with an expected `variationIndex`, `reason`, and `value`), and rollout
   **bucket vectors** (expected exact floats). Expected values are defined by
   [`docs/SPEC.md`](../docs/SPEC.md).
2. `runners/<lang>` — each language loads the corpus, runs it through its own engine,
   and writes `results/<lang>.json`.
3. [`aggregate.py`](aggregate.py) — compares every language's output to the expected
   values and writes [`RESULTS.md`](RESULTS.md), the pass/fail matrix. CI commits the
   refreshed matrix back to `main` on every push.

## Run locally

```bash
# one runner per language (examples)
python conformance/runners/run.py
node   conformance/runners/run.js
dotnet run --project conformance/runners/dotnet
(cd conformance/runners/go && go run .)
cargo run --manifest-path conformance/runners/rust/Cargo.toml
# java: compile the SDK + Runner.java with javac, then run Runner

# then build the matrix
python conformance/aggregate.py
```

A mismatch in any language fails CI — that's the guarantee that a user lands in the
same rollout slice and sees the same variation no matter which SDK their service uses.
