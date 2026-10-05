# Releasing & launch checklist

Switchboard publishes **one ecosystem at a time** via the manual
[`publish.yml`](.github/workflows/publish.yml) workflow (a dropdown picks the target).
Every job is idempotent — re-running after a published version is a no-op.

## 1. Accounts & namespaces to claim (one-time)

| Asset | Name | Notes |
|-------|------|-------|
| GitHub org | **switchboard-io** | Also the Go module path & Maven namespace. |
| Domain | **switchboard.co** + **switchboard.sh** | At-cost registrar (Cloudflare/Porkbun); lock + auto-renew. |
| NuGet | org **Switchboard** (or `switchboard-io`) | Owns `Switchboard.*` — request an ID prefix reservation. |
| npm | org **@switchboard** | Unlocks the `@switchboard/*` scope. |
| PyPI | project `switchboard-sdk` + `switchboard-openfeature-provider` | Use Trusted Publishing (OIDC) — no token. |
| crates.io | `switchboard-sdk` + `switchboard-openfeature` | Needs a crates.io API token. |
| RubyGems | `switchboard-sdk` + `switchboard-openfeature` | Needs a RubyGems API key. |
| Maven Central | namespace `io.github.switchboard-io` | Verify via Sonatype Central by proving GitHub-org ownership (no domain needed). |

> ⚠️ **Trademark knockout first.** "Switchboard" is a common word — run a USPTO/EUIPO
> knockout search in the software classes (9 & 42) before investing in the brand.

## 2. GitHub repo secrets to add

| Secret | Used by |
|--------|---------|
| `NUGET_API_KEY` | nuget job |
| `NPM_TOKEN` | npm job |
| (PyPI) Trusted Publisher | pypi job (configure on PyPI, no secret) |
| `CARGO_REGISTRY_TOKEN` | crates job |
| `RUBYGEMS_API_KEY` | rubygems job |
| `MAVEN_USERNAME`, `MAVEN_PASSWORD`, `MAVEN_GPG_PRIVATE_KEY`, `MAVEN_GPG_PASSPHRASE` | maven job |

## 3. Create the repo & push

```bash
gh repo create switchboard-io/switchboard --public --source=. --remote=origin --push
# or: git remote add origin https://github.com/switchboard-io/switchboard.git && git push -u origin main
```

## 4. Reserve names (publish 0.0.1 everywhere)

Run **Actions → Publish (manual, per ecosystem)** once per target: `nuget`, `npm`,
`pypi`, `crates`, `rubygems`, `maven`, then tag `go-v0.0.1` for Go. This locks every
package name at the current version.

## 5. Pre-1.0 gates (before announcing)

- [ ] `tests.yml` green on all languages.
- [ ] `conformance.yml` green — [`RESULTS.md`](conformance/RESULTS.md) shows all checks passing.
- [ ] `benchmarks.yml` has run at least once (numbers in [BENCHMARKS.md](docs/BENCHMARKS.md)).
- [ ] `pages.yml` deployed the site + playground.
- [ ] All registry badges in the README resolve (green).
- [ ] `scripts/check_versions.py` passes (versions in lockstep).
- [ ] Docs complete: SPEC, GUIDE, BENCHMARKS, design, GTM.

## 6. Launch day

- [ ] Publish the real `1.0.0` across all ecosystems (bump `Directory.Build.props`, each
      manifest, and `scripts/check_versions.py` `EXPECTED`, then run `publish.yml`).
- [ ] Cut a GitHub Release with notes from `CHANGELOG.md`.
- [ ] Announce: Show HN, r/dotnet + language subreddits, the OpenFeature community, a
      launch blog post, and a "migrate from LaunchDarkly" guide.
- [ ] Open Discussions + a community chat; watch issues.

## Version bump cheatsheet

Update the version in **all** of these together (CI enforces it via `check_versions.py`):
`Directory.Build.props` · every `packages/**/package.json` · `pyproject.toml` ·
`Cargo.toml` · `*.gemspec` · `pom.xml` · and tag Go as `go-vX.Y.Z`.
