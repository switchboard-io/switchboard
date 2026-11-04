# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and the project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Toolchain refresh: CI now builds and tests on Node.js 26 (Active LTS), Python 3.15,
  Go 1.27, JDK 25 (LTS), Ruby 4.0 and Rust stable, with the .NET 10 SDK pinned via
  `global.json`. All GitHub Actions moved to their current major versions.
- Raised minimum supported runtimes to currently supported releases: Node.js >= 22,
  Python >= 3.11, Go 1.26 (`go.mod`), Ruby >= 3.3, Rust 1.85 (crates moved to
  edition 2024). Java keeps `release 11`; .NET libraries keep `netstandard2.0`.
- Dependency refresh: xUnit v2 -> xUnit.net v3 4.0 (`xunit.v3.mtp-off`, keeping the
  VSTest `dotnet test` flow), Microsoft.NET.Test.Sdk 18.11, xunit.runner.visualstudio 4.0,
  System.Text.Json 10.0.14 (netstandard2.0 build), Rust `sha1` 0.11.
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
