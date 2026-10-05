# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and the project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Real evaluation engine implementing `docs/SPEC.md` (rules, segments, targets,
  prerequisites, SHA1-based deterministic rollout bucketing).
- Cross-language conformance suite (`conformance/`) with a shared corpus and a
  per-language runner matrix.
- Benchmark harness per language.
- Interactive web playground and expanded docs.
- Control-plane server (REST evaluation + flag management) and docker-compose.

## [0.0.1] - 2026-10-04

### Added
- Initial placeholder packages reserving names across NuGet, npm, PyPI, RubyGems,
  crates.io, Maven Central, and Go modules.
- CI and per-ecosystem release workflows.
- Architecture design and go-to-market plans.
