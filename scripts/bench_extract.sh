#!/usr/bin/env bash
# Extract "throughput : N evals/sec" and "per eval : M ns" from a benchmark log
# and write bench-<lang>.json. Usage: bench_extract.sh <LangLabel> <logfile>
set -euo pipefail
lang="$1"
log="$2"

throughput=$(grep -i 'throughput' "$log" | grep -oE '[0-9,]+' | tr -d ',' | head -1)
ns=$(grep -i 'per eval' "$log" | grep -oE '[0-9.]+' | head -1)

slug=$(echo "$lang" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
cat > "bench-${slug}.json" <<EOF
{ "lang": "${lang}", "nsPerEval": ${ns:-0}, "throughput": ${throughput:-0} }
EOF
echo "wrote bench-${slug}.json: ${throughput:-0} evals/sec"
