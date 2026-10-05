# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and the project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Upgraded all .NET projects (engine, SDK, OpenFeature provider, CLI, server, tests,
  benchmark) to **.NET 10**; libraries continue to multi-target `netstandard2.0` for
  broad consumer reach. Docker images and CI updated to the .NET 10 SDK/runtime.

### Added
- Real evaluation engine implementing `docs/SPEC.md` in **six languages** (.NET, JS,
  Python, Go, Java, Rust) plus Ruby — targeting, segment rules, prerequisites, and
  SHA1-based deterministic rollout bucketing that is byte-identical across all SDKs.
- Cross-language **conformance suite** (`conformance/`) with a shared corpus and a
  per-language runner matrix — **150/150 checks passing across 6 languages**
  (`conformance/RESULTS.md`), committed back by CI on every push.
- **Benchmark harness** per language with measured numbers in `docs/BENCHMARKS.md`.
- **Control-plane server** (ASP.NET Core: REST flag management, server-side evaluation,
  Server-Sent-Events streaming) + Admin UI + `Dockerfile` + `docker-compose.yml`.
- OpenFeature providers wired to the real clients.
- **Web landing page + interactive playground** (`web/`) that runs the exact engine
  (including SHA1 bucketing) in the browser; deployed via GitHub Pages.
- Governance: `NOTICE`, `SECURITY.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`,
  `RELEASING.md`, `scripts/check_versions.py`.
- Workflows reshaped to `tests`, `conformance`, `benchmarks`, `pages`, `publish`.

## [0.0.1] - 2026-10-04

### Added
- Initial placeholder packages reserving names across NuGet, npm, PyPI, RubyGems,
  crates.io, Maven Central, and Go modules.
- CI and per-ecosystem release workflows.
- Architecture design and go-to-market plans.
