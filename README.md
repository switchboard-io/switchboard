# Switchboard

**Self-hosted, open feature-management platform for .NET** — feature flags, targeting, progressive rollouts, kill switches, and experimentation, with no vendor lock-in and built-in flag-lifecycle management.

- 🌐 Website: https://switchboard.co
- 📦 NuGet: `Switchboard`, `Switchboard.Sdk`, `Switchboard.Cli`
- 📖 License: Apache-2.0

> **Status: early scaffolding.** The packages currently published are **placeholder `0.0.1` releases that reserve the package IDs** on NuGet.org while the full implementation is built.

## Repository layout

```
Switchboard/
├─ src/                    # .NET / NuGet packages
│  ├─ Switchboard/         # umbrella package (references the SDK)
│  ├─ Switchboard.Sdk/     # .NET feature-flag SDK (local evaluation)
│  └─ Switchboard.Cli/     # `switchboard` global tool
├─ sdk/                    # multi-language SDK stubs (name reservation)
│  ├─ js/                  # @switchboard/sdk        (npm)
│  ├─ python/              # switchboard-sdk         (PyPI)
│  ├─ go/                  # switchboard-go          (Go modules)
│  ├─ java/                # io.github.switchboard-io:sdk (Maven)
│  ├─ ruby/                # switchboard-sdk         (RubyGems)
│  └─ rust/                # switchboard-sdk         (crates.io)
├─ docs/                   # design + go-to-market plans
├─ assets/                 # shared package icon
├─ Directory.Build.props   # shared NuGet metadata
├─ Switchboard.slnx
└─ LICENSE                 # Apache-2.0
```

## Documentation

- [`docs/Switchboard-Feature-Management-Design.md`](docs/Switchboard-Feature-Management-Design.md) — full architecture, components, sequence diagrams, data model.
- [`docs/Switchboard-GTM-Delivery-Plan.md`](docs/Switchboard-GTM-Delivery-Plan.md) — packaging, NuGet/GitHub, website, signup/onboarding, launch plan.

## Build & pack locally

```bash
dotnet build -c Release
dotnet pack  -c Release -o ./artifacts
```

## Publish (reserve IDs) to NuGet.org

```bash
# After creating a NuGet.org account + API key:
dotnet nuget push "./artifacts/*.nupkg" --api-key <KEY> --source https://api.nuget.org/v3/index.json
```
Then request a **package ID prefix reservation** for `Switchboard.*` on nuget.org.
