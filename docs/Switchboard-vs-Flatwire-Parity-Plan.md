# Switchboard → flatwire parity plan

**What it takes for `switchboard-io/switchboard` to reach the same bar as [`flatwire-io/flatwire`](https://github.com/flatwire-io/flatwire).**

- **Date:** 2026-10-04
- **Owner:** Parag
- **Source scanned:** `flatwire-io/flatwire` @ `main` (201 tree entries, 6 languages, 5 workflows)

---

## 0. The honest headline

**flatwire is a finished, measured, polyglot _library_. Switchboard is, today, a folder of _placeholders_.** The gap is not cosmetic — it's the difference between "reserved the names" and "shipped working software with proof."

Two structural truths to anchor the plan:

1. **flatwire is a pure library** (one streaming-serialization idea, identical API in 6 languages). That's why a *conformance matrix* and *byte-identity proof* are its heart.
2. **Switchboard is a platform** (a server/control-plane + edge relay + client SDKs + CLI). The flatwire playbook maps directly onto Switchboard's **SDKs and providers**, but Switchboard *also* needs the thing flatwire doesn't have: **a running backend**.

So "become flatwire" = **(a)** replace every placeholder with real, tested, benchmarked implementations, **(b)** adopt flatwire's QA/conformance/benchmark/docs/web/publish machinery, and **(c)** additionally build the server that a flag platform requires.

---

## 1. Side-by-side: what flatwire has vs. what we have

| Area | flatwire (target) | Switchboard (today) | Gap |
|------|-------------------|---------------------|-----|
| **Repo layout** | `packages/<lang>/` with real src + tests + bench | `src/` (.NET) + `sdk/` (stubs) | Restructure + fill in |
| **Implementations** | Real, working, in all 6 languages | Placeholder `0.0.1` stubs only | 🔴 Huge — build the actual SDKs |
| **Server/backend** | N/A (library) | Designed, not built | 🔴 Build control plane + relay |
| **Tests** | Per-language test suites | None | 🔴 Add everywhere |
| **Conformance** | Shared `corpus.json` + 6 runners + `aggregate.py` → `RESULTS.md` matrix, CI-committed | None | 🔴 Build equivalent (flag-eval parity) |
| **Benchmarks** | Per-lang bench + `REPORT.md`/`LATENCY.md`, weekly CI, real numbers in README | None | 🔴 Build bench harness |
| **Docs** | 9 `docs/*.md` + architecture SVG+PNG | 3 docs (design, GTM, this) | 🟡 Expand to guide/recipes/formats/etc. |
| **Web** | `web/` static site + **playground** + blog, GitHub Pages | None | 🟡 Build site + playground |
| **README** | Rich, badges to live packages, measured tables, diagram | Basic | 🟡 Rewrite once real |
| **Workflows** | `tests`, `conformance`, `benchmarks`, `pages`, `publish` | `ci` + 7 release | 🟡 Realign to flatwire's 5-workflow shape |
| **Governance files** | LICENSE, NOTICE, SECURITY, CONTRIBUTING, CODE_OF_CONDUCT, CHANGELOG | LICENSE only | 🟡 Add the five missing |
| **Version sync** | `scripts/check_versions.py` across all packages | None | 🟡 Add |
| **Published** | Live on PyPI/npm/NuGet/crates/Maven/pkg.go.dev with badges | Nothing published | 🔴 Publish |
| **On GitHub** | Yes, public org | Local git only (just initialized) | 🟡 Create org + push |

Legend: 🔴 large build effort · 🟡 structured but smaller effort.

---

## 2. How flatwire is built (the pattern to copy)

### 2.1 Directory shape
```
flatwire/
├─ packages/              # one real implementation per language
│  ├─ python/  (flatwire/, tests/, bench/{compare.py,REPORT.md,LATENCY.md,results/})
│  ├─ js/      (src, test/, bench/{compare.js,results/})
│  ├─ dotnet/  (FlatWire/, FlatWire.Tests/, FlatWire.Bench/)
│  ├─ go/      (src, bench/, cmd/bench/)
│  ├─ java/    (src/main, src/test, src/bench, gradle)
│  └─ rust/    (src/, tests/, examples/, bench/)
├─ conformance/           # the cross-language proof
│  ├─ corpus.json         # shared edge-case cases
│  ├─ runners/{python,node,dotnet,go,java,rust}
│  ├─ aggregate.py        # builds the matrix
│  └─ RESULTS.md          # committed back by CI
├─ docs/     (GUIDE, RECIPES, FORMATS, TRANSPORTS, ADAPTERS, BACKPRESSURE, FAILURE, CLI, BENCHMARKS, diagrams/)
├─ web/      (index.html, playground.html + .js, blog, hero image)
├─ scripts/  (check_versions.py)
└─ .github/workflows/ (tests, conformance, benchmarks, pages, publish)
```

### 2.2 The five workflows
| Workflow | Trigger | What it does |
|----------|---------|--------------|
| `tests.yml` | push/PR | Per-language unit tests. |
| `conformance.yml` | push/PR on `packages/**` | Runs corpus through all 6 langs → `aggregate.py` → **commits `RESULTS.md` back to main**. |
| `benchmarks.yml` | manual + weekly cron | Runs every lang's bench on CI (real numbers). |
| `pages.yml` | push | Deploys `web/` to GitHub Pages. |
| `publish.yml` | **manual `workflow_dispatch`**, dropdown per ecosystem | Publishes npm/pypi/nuget/maven one at a time as creds land (with "skip if version exists"). |

### 2.3 The three ideas worth stealing wholesale
1. **A shared conformance corpus + aggregator that CI commits back** → turns "works in all languages" into a visible, enforced matrix.
2. **Benchmarks that actually run in CI** → the README's numbers are reproducible, not marketing.
3. **Manual per-ecosystem publish with idempotent "skip if exists"** → ship registries incrementally as accounts/tokens arrive.

---

## 3. What "Switchboard's conformance" means (the key translation)

flatwire proves **byte-identity of serialized output**. Switchboard's analogous proof is **evaluation parity**: *given the same flag config + evaluation context, every SDK returns the same variation, value, and reason.*

**Build a `conformance/` with:**
- `corpus.json` — a set of `{flagConfig, context} → expectedVariation/value/reason` cases covering: boolean/string/number/JSON flags, individual targets, segment rules, clause operators (equals/in/contains/semver/regex/before-after), percentage rollout **bucketing** (consistent-hash must match across languages — this is the hard one, like flatwire's byte-identity), prerequisites, off-variation, and the full reason enum.
- `runners/{dotnet,js,python,go,java,rust}` — each loads the corpus, runs its SDK's evaluation engine locally, emits `results/<lang>.json`.
- `aggregate.py` → `RESULTS.md` — a matrix: each case × each language = match/mismatch, committed back by CI.

**The bucketing hash is the make-or-break:** every language must implement the *same* hash (e.g., MurmurHash3 of `context.key + ":" + flag.salt`) so a user lands in the same rollout slice everywhere. This is exactly the kind of cross-language subtlety flatwire's matrix is designed to catch.

---

## 4. Gap-closing roadmap (phased)

### Phase A — Foundations & governance (fast, do now)
- [ ] Add governance files flatwire has: **NOTICE, SECURITY.md, CONTRIBUTING.md, CODE_OF_CONDUCT.md, CHANGELOG.md**.
- [ ] Add `scripts/check_versions.py` — assert all package manifests share the version.
- [ ] Create GitHub org **`switchboard-io`** + repo **`switchboard`**, push `main` (see §6).
- [ ] Rewrite `tests/conformance/benchmarks/pages/publish` workflows to flatwire's shape (keep our tag-release option too).
- [ ] Decide repo layout: **adopt `packages/<lang>/`** (flatwire-style) or keep `src/` + `sdk/`. *Recommendation: move to `packages/` for parity.*

### Phase B — Make the .NET implementation real (our strongest language)
- [ ] Build the actual `Switchboard.Evaluation` engine (rules, segments, bucketing, reasons) + unit tests.
- [ ] Real `Switchboard.Sdk` (local eval + streaming client + offline cache) + tests.
- [ ] Real `Switchboard.OpenFeature` provider + tests.
- [ ] Real `switchboard` CLI (System.CommandLine) with the designed commands.
- [ ] .NET benchmark project producing real eval-latency/throughput numbers.

### Phase C — The server (what flatwire doesn't need, but we do)
- [ ] Control plane (ASP.NET Core: REST + gRPC), PostgreSQL, Redis fan-out.
- [ ] Edge Relay (SSE/gRPC streaming).
- [ ] Minimal Admin UI + first-run wizard.
- [ ] `docker-compose.yml` + `all-in-one` image + Helm chart.

### Phase D — Port to the other five languages (flatwire parity)
- [ ] Real SDK + provider in **JS, Python, Go, Java, Rust**, each with tests + bench.
- [ ] Each wraps the *same* evaluation semantics (shared corpus keeps them honest).

### Phase E — Conformance + benchmarks (the proof)
- [ ] Build `conformance/` corpus + 6 runners + `aggregate.py` → `RESULTS.md` (§3).
- [ ] Wire `conformance.yml` to run on every push and commit the matrix back.
- [ ] Build per-language benchmark harness + `benchmarks.yml` (weekly), publish numbers to `docs/BENCHMARKS.md`.

### Phase F — Docs + web + launch polish
- [ ] Expand `docs/`: GUIDE, RECIPES, TARGETING, ROLLOUTS, SELF-HOSTING, SDK-per-language, CLI, FORMATS-of-config, FAILURE/offline, BENCHMARKS + architecture diagram (SVG+PNG).
- [ ] Build `web/`: landing + **interactive flag playground** (evaluate a flag against a context in-browser) + blog + Pages deploy.
- [ ] Rewrite root README flatwire-style: badges to live packages, measured tables, architecture image, 60-sec quickstart per language.

### Phase G — Publish & announce
- [ ] Reserve/publish on all six registries via `publish.yml` as tokens land.
- [ ] Verify all badges go green (PyPI/npm/NuGet/crates/Maven/pkg.go.dev + CI + conformance).
- [ ] Launch (HN/Show HN, r/dotnet, OpenFeature community) per the GTM plan.

---

## 5. Effort reality check

| Phase | Rough size | Notes |
|-------|-----------|-------|
| A — Foundations | **S** | Files + workflows + org/push. Days. |
| B — Real .NET | **L** | The actual engine + SDK + CLI + tests + bench. |
| C — Server | **XL** | A product in itself; flatwire has no equivalent. |
| D — 5 more languages | **XL** | 5× the SDK/provider work, kept honest by conformance. |
| E — Conformance + bench | **M–L** | Mechanics are flatwire-proven; bucketing parity is the risk. |
| F — Docs + web | **M** | Playground is the standout piece. |
| G — Publish | **S–M** | Mostly accounts, tokens, and green badges. |

**Critical path:** B → E (a real engine + the conformance harness) is what converts Switchboard from "names reserved" to "flatwire-grade proof." The server (C) is parallelizable and only needed for the *hosted/streaming* story, not for the SDK conformance proof.

---

## 6. Getting it onto GitHub as `switchboard-io` (actionable now)

Local git is initialized (`main`, first commit done). To publish:

**Option A — GitHub CLI (fastest):**
```bash
# one-time: gh auth login
gh repo create switchboard-io/switchboard --public --source=. --remote=origin --push
```
> If the org doesn't exist yet, create it first at https://github.com/organizations/new (free), name **switchboard-io**, then run the command above.

**Option B — manual:**
```bash
# After creating the org + empty repo "switchboard" on github.com:
git remote add origin https://github.com/switchboard-io/switchboard.git
git push -u origin main
```

**Why `switchboard-io` matters beyond the name:** it's also the **Go module path** (`github.com/switchboard-io/switchboard-go`) and the **Maven namespace** (`io.github.switchboard-io`), both already baked into our manifests — so claiming this exact org unlocks those registries without a domain.

---

## 7. Recommended immediate next actions

1. **Create the `switchboard-io` org + push** (you: org/auth; me: everything up to the push).
2. **Phase A files** — I can scaffold NOTICE, SECURITY, CONTRIBUTING, CODE_OF_CONDUCT, CHANGELOG, `check_versions.py`, and reshape the workflows to flatwire's 5-file pattern right now.
3. **Pick the critical-path language** — confirm .NET as the first *real* implementation, then I start Phase B (engine + tests + bench) so we have one language fully flatwire-grade before porting.

---

*flatwire's lesson in one line: the moat isn't the idea, it's the **proof** — identical behavior across six languages, measured in CI, published with green badges. Switchboard already has the names and the design; the work ahead is replacing placeholders with real, conformance-proven, benchmarked implementations and the backend a flag platform requires.*
