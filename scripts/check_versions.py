#!/usr/bin/env python3
"""Assert every Switchboard package manifest declares the same version.

Run in CI (and locally before a release) so the six languages never drift.
Exit code 0 = all in sync; 1 = mismatch.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
EXPECTED = "0.0.1"  # bump here when releasing


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def find_versions() -> "dict[str, str]":
    found: "dict[str, str]" = {}

    props = ROOT / "Directory.Build.props"
    if props.exists():
        m = re.search(r"<Version>([^<]+)</Version>", read(props))
        if m:
            found["dotnet/Directory.Build.props"] = m.group(1).strip()

    for pj in ROOT.glob("packages/**/package.json"):
        if "node_modules" in pj.parts:
            continue
        found[str(pj.relative_to(ROOT))] = json.loads(read(pj)).get("version", "?")

    for pp in ROOT.glob("packages/**/pyproject.toml"):
        m = re.search(r'(?m)^version\s*=\s*"([^"]+)"', read(pp))
        if m:
            found[str(pp.relative_to(ROOT))] = m.group(1)

    for ct in ROOT.glob("packages/**/Cargo.toml"):
        m = re.search(r'(?m)^version\s*=\s*"([^"]+)"', read(ct))
        if m:
            found[str(ct.relative_to(ROOT))] = m.group(1)

    for gs in ROOT.glob("packages/**/*.gemspec"):
        m = re.search(r'\.version\s*=\s*"([^"]+)"', read(gs))
        if m:
            found[str(gs.relative_to(ROOT))] = m.group(1)

    for pom in ROOT.glob("packages/**/pom.xml"):
        m = re.search(r"<version>([^<]+)</version>", read(pom))
        if m:
            found[str(pom.relative_to(ROOT))] = m.group(1).strip()

    return found


def main() -> int:
    versions = find_versions()
    if not versions:
        print("No manifests found.")
        return 1

    width = max(len(k) for k in versions)
    mismatched = {k: v for k, v in versions.items() if v != EXPECTED}
    for k, v in sorted(versions.items()):
        mark = "OK " if v == EXPECTED else "!! "
        print(f"{mark}{k.ljust(width)}  {v}")

    if mismatched:
        print(f"\nExpected all = {EXPECTED}. Mismatches: {len(mismatched)}")
        return 1
    print(f"\nAll {len(versions)} manifests at {EXPECTED}.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
