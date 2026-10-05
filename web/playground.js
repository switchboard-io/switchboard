// Switchboard browser evaluation engine — a faithful port of docs/SPEC.md for the
// playground. Includes a small pure-JS SHA1 so bucketing is byte-identical to the SDKs.

// ---- SHA1 (pure JS, returns lowercase hex) ----
function sha1(str) {
  function rotl(n, s) { return (n << s) | (n >>> (32 - s)); }
  const bytes = new TextEncoder().encode(str);
  const ml = bytes.length * 8;
  const withOne = new Uint8Array(((bytes.length + 8) >> 6) * 64 + 64);
  withOne.set(bytes);
  withOne[bytes.length] = 0x80;
  const dv = new DataView(withOne.buffer);
  dv.setUint32(withOne.length - 4, ml >>> 0, false);
  dv.setUint32(withOne.length - 8, Math.floor(ml / 0x100000000), false);

  let h0 = 0x67452301, h1 = 0xEFCDAB89, h2 = 0x98BADCFE, h3 = 0x10325476, h4 = 0xC3D2E1F0;
  const w = new Int32Array(80);
  for (let off = 0; off < withOne.length; off += 64) {
    for (let i = 0; i < 16; i++) w[i] = dv.getInt32(off + i * 4, false);
    for (let i = 16; i < 80; i++) w[i] = rotl(w[i - 3] ^ w[i - 8] ^ w[i - 14] ^ w[i - 16], 1);
    let a = h0, b = h1, c = h2, d = h3, e = h4;
    for (let i = 0; i < 80; i++) {
      let f, k;
      if (i < 20) { f = (b & c) | (~b & d); k = 0x5A827999; }
      else if (i < 40) { f = b ^ c ^ d; k = 0x6ED9EBA1; }
      else if (i < 60) { f = (b & c) | (b & d) | (c & d); k = 0x8F1BBCDC; }
      else { f = b ^ c ^ d; k = 0xCA62C1D6; }
      const t = (rotl(a, 5) + f + e + k + w[i]) | 0;
      e = d; d = c; c = rotl(b, 30); b = a; a = t;
    }
    h0 = (h0 + a) | 0; h1 = (h1 + b) | 0; h2 = (h2 + c) | 0; h3 = (h3 + d) | 0; h4 = (h4 + e) | 0;
  }
  const toHex = (n) => (n >>> 0).toString(16).padStart(8, "0");
  return toHex(h0) + toHex(h1) + toHex(h2) + toHex(h3) + toHex(h4);
}

const MAX = 0xFFFFFFFFFFFFF; // 2^52 - 1
export function bucketOf(flagKey, salt, contextKey) {
  const hex = sha1(`${flagKey}.${salt}.${contextKey}`);
  return parseInt(hex.slice(0, 13), 16) / MAX;
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
  if (attr == null) return false;
  switch (op) {
    case "in": return values.some((v) => scalarEquals(attr, v));
    case "contains": { const s = asString(attr); return s != null && values.some((v) => { const t = asString(v); return t != null && s.includes(t); }); }
    case "startsWith": { const s = asString(attr); return s != null && values.some((v) => { const t = asString(v); return t != null && s.startsWith(t); }); }
    case "endsWith": { const s = asString(attr); return s != null && values.some((v) => { const t = asString(v); return t != null && s.endsWith(t); }); }
    case "greaterThan": { const a = asNumber(attr), b = asNumber(values[0]); return a != null && b != null && a > b; }
    case "lessThan": { const a = asNumber(attr), b = asNumber(values[0]); return a != null && b != null && a < b; }
    case "regexMatch": { const s = asString(attr), p = asString(values[0]); return s != null && p != null && new RegExp(p).test(s); }
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
export function evaluate(flag, ctx, resolver) {
  try {
    if (!flag.enabled) return result(flag, flag.offVariation, "OFF");
    for (const p of flag.prerequisites || []) {
      const pre = resolver ? resolver(p.key) : null;
      if (!pre || evaluate(pre, ctx, resolver).variationIndex !== p.variation)
        return result(flag, flag.offVariation, "PREREQUISITE_FAILED");
    }
    for (const t of flag.targets || []) if ((t.values || []).includes(ctx.key)) return result(flag, t.variation, "TARGET_MATCH");
    for (const rule of flag.rules || [])
      if ((rule.clauses || []).every((c) => matchClause(c, ctx)))
        return result(flag, resolveIndex(flag, rule.variation, rule.rollout, ctx), "RULE_MATCH");
    const ft = flag.fallthrough || {};
    return result(flag, resolveIndex(flag, ft.variation, ft.rollout, ctx), "FALLTHROUGH");
  } catch { return { value: null, variationIndex: -1, reason: "ERROR" }; }
}

// ---- UI wiring ----
function run() {
  const flagEl = document.getElementById("flag");
  const ctxEl = document.getElementById("ctx");
  const out = document.getElementById("out");
  try {
    const flag = JSON.parse(flagEl.value);
    const ctx = JSON.parse(ctxEl.value);
    const r = evaluate(flag, ctx);
    const bucket = bucketOf(flag.key, flag.salt || "", ctx.key);
    out.className = "ok";
    out.textContent =
      `value          = ${JSON.stringify(r.value)}\n` +
      `variationIndex = ${r.variationIndex}\n` +
      `reason         = ${r.reason}\n` +
      `rollout bucket = ${bucket}`;
  } catch (e) {
    out.className = "err";
    out.textContent = "Error: " + e.message;
  }
}

if (typeof window !== "undefined") {
  window.addEventListener("DOMContentLoaded", () => {
    document.getElementById("run").addEventListener("click", run);
    run();
  });
}
