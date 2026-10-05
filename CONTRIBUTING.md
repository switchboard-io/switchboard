# Contributing to Switchboard

Thanks for your interest in Switchboard! This repository is a polyglot monorepo:
one feature-management platform with SDKs in six languages that must all behave
identically.

## Ground rules

1. **The spec is law.** All evaluation behavior is defined in
   [`docs/SPEC.md`](docs/SPEC.md). Any change to evaluation semantics must update the
   spec **and** every language implementation **and** the conformance corpus together.
2. **Conformance must stay green.** The shared corpus in `conformance/corpus.json`
   runs through every language on every push and produces
   [`conformance/RESULTS.md`](conformance/RESULTS.md). A PR that makes any language
   diverge will be rejected.
3. **Versions stay in lockstep.** `scripts/check_versions.py` asserts every package
   manifest shares the same version. Bump them together.

## Layout

```
packages/<lang>/     real SDK + OpenFeature provider per language
conformance/         shared corpus + per-language runners + aggregator
docs/                spec, guides, benchmarks
web/                 marketing site + interactive playground
scripts/             repo tooling (version check, release helpers)
```

## Local development

- **.NET:** `dotnet build -c Release && dotnet test`
- **JS:** `node packages/js/test/run.js`
- **Python:** `python -m pytest packages/python/tests`
- **Rust:** `cargo test --manifest-path packages/rust/Cargo.toml`
- **Go:** `go test ./...` (in `packages/go`)
- **Java:** `gradle test` (in `packages/java`)

Run the conformance suite locally with `python conformance/aggregate.py` after
generating per-language result files (see `conformance/README.md`).

## Pull requests

- Keep PRs focused; include tests.
- Describe the behavior change and link the spec section it touches.
- By contributing you agree your work is licensed under Apache-2.0.
