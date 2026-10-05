// @switchboard/sdk — local-evaluation feature-flag engine.
// Implements docs/SPEC.md identically to the other language SDKs.
import { createHash } from "crypto";

const MAX = 0xFFFFFFFFFFFFF; // 2^52 - 1

/** Deterministic rollout bucket in [0,1) — identical across all SDKs (SPEC §4). */
export function bucketOf(flagKey, salt, contextKey) {
  const input = `${flagKey}.${salt}.${contextKey}`;
  const hex = createHash("sha1").update(input, "utf8").digest("hex");
  const n = parseInt(hex.slice(0, 13), 16); // 52 bits -> exact double
  return n / MAX;
}

const asNumber = (v) => (typeof v === "number" ? v : null);
function asString(v) {
  if (typeof v === "string") return v;
  if (typeof v === "number") return String(v);
  if (typeof v === "boolean") return v ? "true" : "false";
  return null;
}
function scalarEquals(a, b) {
  if (typeof a === "number" && typeof b === "number") return a === b;
  if (typeof a === "boolean" && typeof b === "boolean") return a === b;
  return asString(a) === asString(b);
}

function matchOp(op, attr, values) {
  if (attr === undefined || attr === null) return false;
  switch (op) {
    case "in": return values.some((v) => scalarEquals(attr, v));
    case "contains": { const s = asString(attr); return s !== null && values.some((v) => { const t = asString(v); return t !== null && s.includes(t); }); }
    case "startsWith": { const s = asString(attr); return s !== null && values.some((v) => { const t = asString(v); return t !== null && s.startsWith(t); }); }
    case "endsWith": { const s = asString(attr); return s !== null && values.some((v) => { const t = asString(v); return t !== null && s.endsWith(t); }); }
    case "greaterThan": { const a = asNumber(attr), b = asNumber(values[0]); return a !== null && b !== null && a > b; }
    case "lessThan": { const a = asNumber(attr), b = asNumber(values[0]); return a !== null && b !== null && a < b; }
    case "regexMatch": { const s = asString(attr), p = asString(values[0]); return s !== null && p !== null && new RegExp(p).test(s); }
    default: return false;
  }
}

function matchClause(clause, ctx) {
  const attr = clause.attribute === "key" ? ctx.key : (ctx.attributes || {})[clause.attribute];
  const r = matchOp(clause.op, attr, clause.values || []);
  return clause.negate ? !r : r;
}

function resolveIndex(flag, variation, rollout, ctx) {
  if (variation !== undefined && variation !== null) return variation;
  if (rollout && rollout.length) {
    const bucket = bucketOf(flag.key, flag.salt || "", ctx.key);
    let cum = 0;
    for (const wv of rollout) { cum += wv.weight; if (bucket * 100000 < cum) return wv.variation; }
    return rollout[rollout.length - 1].variation;
  }
  throw new Error("rule has neither variation nor rollout");
}

function result(flag, index, reason) {
  if (index < 0 || index >= flag.variations.length) return { value: null, variationIndex: -1, reason: "ERROR" };
  return { value: flag.variations[index], variationIndex: index, reason };
}

/** Evaluate a flag for a context. `resolver(key)` returns another flag for prerequisites. */
export function evaluate(flag, ctx, resolver) {
  try {
    if (!flag.enabled) return result(flag, flag.offVariation, "OFF");
    for (const p of flag.prerequisites || []) {
      const pre = resolver ? resolver(p.key) : null;
      if (!pre) return result(flag, flag.offVariation, "PREREQUISITE_FAILED");
      if (evaluate(pre, ctx, resolver).variationIndex !== p.variation)
        return result(flag, flag.offVariation, "PREREQUISITE_FAILED");
    }
    for (const t of flag.targets || []) if ((t.values || []).includes(ctx.key)) return result(flag, t.variation, "TARGET_MATCH");
    for (const rule of flag.rules || []) {
      if ((rule.clauses || []).every((c) => matchClause(c, ctx)))
        return result(flag, resolveIndex(flag, rule.variation, rule.rollout, ctx), "RULE_MATCH");
    }
    const ft = flag.fallthrough || {};
    return result(flag, resolveIndex(flag, ft.variation, ft.rollout, ctx), "FALLTHROUGH");
  } catch {
    return { value: null, variationIndex: -1, reason: "ERROR" };
  }
}

/** A client over a snapshot: { flags: [ ...FlagConfig ] }. */
export class SwitchboardClient {
  constructor(flags) { this.flags = new Map(flags.map((f) => [f.key, f])); }
  static fromJson(json) { const d = typeof json === "string" ? JSON.parse(json) : json; return new SwitchboardClient(d.flags || []); }
  evaluateDetail(key, ctx) {
    const flag = this.flags.get(key);
    if (!flag) return { value: null, variationIndex: -1, reason: "ERROR" };
    return evaluate(flag, ctx, (k) => this.flags.get(k) || null);
  }
  getBool(key, ctx, def = false) { const v = this.evaluateDetail(key, ctx).value; return typeof v === "boolean" ? v : def; }
  getString(key, ctx, def = "") { const v = this.evaluateDetail(key, ctx).value; return typeof v === "string" ? v : def; }
  getNumber(key, ctx, def = 0) { const v = this.evaluateDetail(key, ctx).value; return typeof v === "number" ? v : def; }
}

export default { bucketOf, evaluate, SwitchboardClient };