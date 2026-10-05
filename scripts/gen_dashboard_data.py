#!/usr/bin/env python3
"""Regenerate web/data.json from the live conformance results.

Run by CI after the conformance matrix is aggregated, so the public dashboard's
numbers always reflect the latest proven state. Benchmark figures are read from an
optional web/bench.json (written by the benchmarks workflow); if absent, the existing
benchmark block in data.json is preserved so we never publish stale-but-wrong numbers
as if fresh.
"""
import datetime
import glob
import json
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CONF = os.path.join(ROOT, "conformance")
WEB = os.path.join(ROOT, "web")


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def main():
    corpus = load(os.path.join(CONF, "corpus.json"))

    out_path = os.path.join(WEB, "data.json")
    existing = load(out_path) if os.path.exists(out_path) else {}

    # Discover which languages produced results this run.
    result_files = sorted(glob.glob(os.path.join(CONF, "results", "*.json")))
    langs = [load(p)["lang"] for p in result_files]

    cases = len(corpus["cases"])
    buckets = len(corpus["buckets"])
    checks_per_lang = cases + buckets

    if result_files:
        # Fresh conformance results present — recompute the matrix numbers.
        expected_cases = {c["id"]: c["expect"] for c in corpus["cases"]}
        expected_buckets = {f"{b['flagKey']}.{b['salt']}.{b['contextKey']}": b["expected"] for b in corpus["buckets"]}
        total = checks_per_lang * len(langs)
        passing = 0
        for p in result_files:
            data = load(p)
            for c in data["cases"]:
                exp = expected_cases.get(c["id"])
                if exp and c["variationIndex"] == exp["variationIndex"] and c["reason"] == exp["reason"] and c["value"] == exp["value"]:
                    passing += 1
            for b in data["buckets"]:
                if b["id"] in expected_buckets and b["actual"] == expected_buckets[b["id"]]:
                    passing += 1
        conformance = {
            "total": total,
            "passing": passing,
            "checksPerLang": checks_per_lang,
            "cases": cases,
            "buckets": buckets,
            "languages": langs,
        }
    else:
        # No fresh results (e.g. the benchmark job) — keep the last published matrix
        # so we never clobber a valid 150/150 with 0/0.
        conformance = existing.get("conformance", {
            "total": 0, "passing": 0, "checksPerLang": checks_per_lang,
            "cases": cases, "buckets": buckets, "languages": [],
        })

    # Benchmarks: prefer a fresh web/bench.json, else keep what's already published.
    bench_path = os.path.join(WEB, "bench.json")
    if os.path.exists(bench_path):
        benchmarks = load(bench_path)
    else:
        benchmarks = existing.get("benchmarks", [])

    data = {
        "generated": datetime.date.today().isoformat(),
        "conformance": conformance,
        "benchmarks": benchmarks,
        "buckets": [
            {"vector": f"{b['flagKey']}.{b['salt']}.{b['contextKey']}", "value": b["expected"]}
            for b in corpus["buckets"]
        ],
    }

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
        f.write("\n")
    print(f"web/data.json refreshed: conformance {conformance['passing']}/{conformance['total']}, {len(benchmarks)} benchmark rows")


if __name__ == "__main__":
    main()
