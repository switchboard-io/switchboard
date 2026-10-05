// Conformance runner (Node). Evaluates the shared corpus and writes results.
import { readFileSync, mkdirSync, writeFileSync } from "fs";
import { dirname, join } from "path";
import { fileURLToPath } from "url";
import { SwitchboardClient, bucketOf } from "../../packages/js/index.js";

const here = dirname(fileURLToPath(import.meta.url));
const root = dirname(here); // conformance/
const corpus = JSON.parse(readFileSync(join(root, "corpus.json"), "utf8"));

const client = SwitchboardClient.fromJson({ flags: corpus.flags });

const cases = corpus.cases.map((c) => {
  const r = client.evaluateDetail(c.flag, c.context);
  return { id: c.id, variationIndex: r.variationIndex, reason: r.reason, value: r.value };
});

const buckets = corpus.buckets.map((b) => ({
  id: `${b.flagKey}.${b.salt}.${b.contextKey}`,
  actual: bucketOf(b.flagKey, b.salt, b.contextKey),
}));

const outDir = join(root, "results");
mkdirSync(outDir, { recursive: true });
writeFileSync(join(outDir, "node.json"), JSON.stringify({ lang: "node", cases, buckets }, null, 2));
console.log(`node runner: ${cases.length} cases, ${buckets.length} buckets`);
