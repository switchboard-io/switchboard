#!/usr/bin/env python3
"""Aggregate per-language conformance results into a cross-language matrix.

Reads corpus.json (expected) and results/<lang>.json (actual), then writes
RESULTS.md: for every case and bucket, whether each language matches the
expected value. This is Switchboard's proof that all SDKs evaluate identically.
"""
import glob
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
LANGS_ORDER = ["dotnet", "node", "python", "go", "java", "rust", "ruby"]
BUCKET_TOL = 0.0  # exact match required (52-bit integer -> exact double)


def load_json(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def main():
    corpus = load_json(os.path.join(HERE, "corpus.json"))
    expected_cases = {c["id"]: c["expect"] for c in corpus["cases"]}
    expected_buckets = {f"{b['flagKey']}.{b['salt']}.{b['contextKey']}": b["expected"] for b in corpus["buckets"]}

    results = {}
    for path in sorted(glob.glob(os.path.join(HERE, "results", "*.json"))):
        data = load_json(path)
        results[data["lang"]] = data

    langs = [l for l in LANGS_ORDER if l in results] + [l for l in results if l not in LANGS_ORDER]

    def case_match(lang, cid):
        got = next((c for c in results[lang]["cases"] if c["id"] == cid), None)
        if got is None:
            return None
        exp = expected_cases[cid]
        return got["variationIndex"] == exp["variationIndex"] and got["reason"] == exp["reason"] and got["value"] == exp["value"]

    def bucket_match(lang, bid):
        got = next((b for b in results[lang]["buckets"] if b["id"] == bid), None)
        if got is None:
            return None
        return abs(got["actual"] - expected_buckets[bid]) <= BUCKET_TOL

    lines = []
    lines.append("# Conformance results\n")
    lines.append("Every Switchboard SDK evaluates the shared corpus in `corpus.json`. This matrix,")
    lines.append("regenerated in CI on every push, proves they all agree with the expected result")
    lines.append("defined by [`docs/SPEC.md`](../docs/SPEC.md).\n")

    total = 0
    passed = 0

    def cell(ok):
        return "✅" if ok else ("—" if ok is None else "❌")

    # Evaluation cases
    lines.append("## Evaluation cases\n")
    header = "| Case | Expected | " + " | ".join(langs) + " |"
    sep = "|------|----------|" + "|".join(["----"] * len(langs)) + "|"
    lines.append(header)
    lines.append(sep)
    for c in corpus["cases"]:
        cid = c["id"]
        exp = expected_cases[cid]
        row_cells = []
        for lang in langs:
            ok = case_match(lang, cid)
            row_cells.append(cell(ok))
            if ok is not None:
                total += 1
                passed += 1 if ok else 0
        exp_str = f"`{exp['reason']}` / `{json.dumps(exp['value'])}`"
        lines.append(f"| `{cid}` | {exp_str} | " + " | ".join(row_cells) + " |")

    # Bucket vectors
    lines.append("\n## Rollout bucket vectors (exact cross-language identity)\n")
    header = "| Vector | Expected | " + " | ".join(langs) + " |"
    sep = "|--------|----------|" + "|".join(["----"] * len(langs)) + "|"
    lines.append(header)
    lines.append(sep)
    for b in corpus["buckets"]:
        bid = f"{b['flagKey']}.{b['salt']}.{b['contextKey']}"
        row_cells = []
        for lang in langs:
            ok = bucket_match(lang, bid)
            row_cells.append(cell(ok))
            if ok is not None:
                total += 1
                passed += 1 if ok else 0
        lines.append(f"| `{bid}` | `{b['expected']}` | " + " | ".join(row_cells) + " |")

    status = "ALL PASSING" if passed == total and total > 0 else f"{total - passed} MISMATCH(ES)"
    lines.insert(4, f"**Status: {passed}/{total} checks passing across {len(langs)} languages — {status}.**\n")

    out = os.path.join(HERE, "RESULTS.md")
    with open(out, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    print(f"Aggregated {len(langs)} languages: {passed}/{total} checks passing.")
    if passed != total:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
