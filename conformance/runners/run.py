#!/usr/bin/env python3
"""Conformance runner (Python). Evaluates the shared corpus and writes results."""
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)  # conformance/
sys.path.insert(0, os.path.join(os.path.dirname(ROOT), "packages", "python"))

from switchboard_sdk import SwitchboardClient, bucket_of  # noqa: E402


def main() -> None:
    with open(os.path.join(ROOT, "corpus.json"), encoding="utf-8") as f:
        corpus = json.load(f)

    client = SwitchboardClient.from_json({"flags": corpus["flags"]})

    cases = []
    for c in corpus["cases"]:
        r = client.evaluate_detail(c["flag"], c["context"])
        cases.append({
            "id": c["id"],
            "variationIndex": r["variationIndex"],
            "reason": r["reason"],
            "value": r["value"],
        })

    buckets = [
        {"id": f"{b['flagKey']}.{b['salt']}.{b['contextKey']}",
         "actual": bucket_of(b["flagKey"], b["salt"], b["contextKey"])}
        for b in corpus["buckets"]
    ]

    out_dir = os.path.join(ROOT, "results")
    os.makedirs(out_dir, exist_ok=True)
    with open(os.path.join(out_dir, "python.json"), "w", encoding="utf-8") as f:
        json.dump({"lang": "python", "cases": cases, "buckets": buckets}, f, indent=2)
    print(f"python runner: {len(cases)} cases, {len(buckets)} buckets")


if __name__ == "__main__":
    main()
