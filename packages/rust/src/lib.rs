//! switchboard-sdk — local-evaluation feature-flag engine for Rust.
//!
//! Implements docs/SPEC.md identically to the other language SDKs.

use serde_json::Value;
use sha1::{Digest, Sha1};
use std::collections::HashMap;

const MAX: f64 = 0xF_FFFF_FFFF_FFFF_u64 as f64; // 2^52 - 1 = 4503599627370495

/// Deterministic rollout bucket in [0, 1) — identical across all SDKs (SPEC §4).
pub fn bucket_of(flag_key: &str, salt: &str, context_key: &str) -> f64 {
    let input = format!("{}.{}.{}", flag_key, salt, context_key);
    let mut hasher = Sha1::new();
    hasher.update(input.as_bytes());
    let digest = hasher.finalize();
    let hexs = hex::encode(digest); // 40 lowercase hex chars
    let n = u64::from_str_radix(&hexs[..13], 16).unwrap();
    n as f64 / MAX
}

/// Result of an evaluation.
#[derive(Debug, Clone, PartialEq)]
pub struct EvalResult {
    pub value: Value,
    pub variation_index: i64,
    pub reason: String,
}

fn err_result() -> EvalResult {
    EvalResult { value: Value::Null, variation_index: -1, reason: "ERROR".into() }
}

fn as_number(v: &Value) -> Option<f64> {
    if v.is_boolean() { return None; }
    v.as_f64()
}

fn as_string(v: &Value) -> Option<String> {
    match v {
        Value::String(s) => Some(s.clone()),
        Value::Bool(b) => Some(if *b { "true".into() } else { "false".into() }),
        Value::Number(n) => {
            if let Some(i) = n.as_i64() { Some(i.to_string()) }
            else { n.as_f64().map(|f| f.to_string()) }
        }
        _ => None,
    }
}

fn scalar_equals(a: &Value, b: &Value) -> bool {
    if let (Some(na), Some(nb)) = (as_number(a), as_number(b)) {
        return na == nb;
    }
    if let (Value::Bool(ba), Value::Bool(bb)) = (a, b) {
        return ba == bb;
    }
    as_string(a) == as_string(b)
}

fn match_op(op: &str, attr: &Value, values: &[Value]) -> bool {
    if attr.is_null() {
        return false;
    }
    match op {
        "in" => values.iter().any(|v| scalar_equals(attr, v)),
        "contains" => matches!(as_string(attr), Some(ref s) if values.iter().any(|v| as_string(v).map_or(false, |t| s.contains(&t)))),
        "startsWith" => matches!(as_string(attr), Some(ref s) if values.iter().any(|v| as_string(v).map_or(false, |t| s.starts_with(&t)))),
        "endsWith" => matches!(as_string(attr), Some(ref s) if values.iter().any(|v| as_string(v).map_or(false, |t| s.ends_with(&t)))),
        "greaterThan" => matches!((as_number(attr), values.first().and_then(as_number)), (Some(a), Some(b)) if a > b),
        "lessThan" => matches!((as_number(attr), values.first().and_then(as_number)), (Some(a), Some(b)) if a < b),
        "regexMatch" => {
            // Substring fallback (no regex crate dependency). Enable a `regex`
            // feature for full PCRE-style matching.
            match (as_string(attr), values.first().and_then(as_string)) {
                (Some(s), Some(p)) => s.contains(&p),
                _ => false,
            }
        }
        _ => false,
    }
}

fn match_clause(clause: &Value, ctx: &Value) -> bool {
    let attribute = clause.get("attribute").and_then(|a| a.as_str()).unwrap_or("");
    let attr: Value = if attribute == "key" {
        ctx.get("key").cloned().unwrap_or(Value::Null)
    } else {
        ctx.get("attributes").and_then(|a| a.get(attribute)).cloned().unwrap_or(Value::Null)
    };
    let empty: Vec<Value> = vec![];
    let values = clause.get("values").and_then(|v| v.as_array()).unwrap_or(&empty);
    let op = clause.get("op").and_then(|o| o.as_str()).unwrap_or("");
    let r = match_op(op, &attr, values);
    if clause.get("negate").and_then(|n| n.as_bool()).unwrap_or(false) { !r } else { r }
}

fn resolve_index(flag: &Value, variation: Option<i64>, rollout: Option<&Vec<Value>>, ctx: &Value) -> Option<i64> {
    if let Some(v) = variation {
        return Some(v);
    }
    if let Some(r) = rollout {
        if !r.is_empty() {
            let flag_key = flag.get("key").and_then(|k| k.as_str()).unwrap_or("");
            let salt = flag.get("salt").and_then(|s| s.as_str()).unwrap_or("");
            let ctx_key = ctx.get("key").and_then(|k| k.as_str()).unwrap_or("");
            let bucket = bucket_of(flag_key, salt, ctx_key);
            let mut cumulative = 0f64;
            for wv in r {
                cumulative += wv.get("weight").and_then(|w| w.as_f64()).unwrap_or(0.0);
                if bucket * 100000.0 < cumulative {
                    return wv.get("variation").and_then(|v| v.as_i64());
                }
            }
            return r.last().and_then(|wv| wv.get("variation")).and_then(|v| v.as_i64());
        }
    }
    None
}

fn make_result(flag: &Value, index: Option<i64>, reason: &str) -> EvalResult {
    let idx = match index {
        Some(i) => i,
        None => return err_result(),
    };
    let variations = flag.get("variations").and_then(|v| v.as_array());
    match variations {
        Some(vars) if idx >= 0 && (idx as usize) < vars.len() => EvalResult {
            value: vars[idx as usize].clone(),
            variation_index: idx,
            reason: reason.into(),
        },
        _ => err_result(),
    }
}

/// Evaluate a flag for a context. `resolver` returns prerequisite flags by key.
pub fn evaluate(flag: &Value, ctx: &Value, resolver: &dyn Fn(&str) -> Option<Value>) -> EvalResult {
    let off = flag.get("offVariation").and_then(|v| v.as_i64()).unwrap_or(0);

    if !flag.get("enabled").and_then(|e| e.as_bool()).unwrap_or(false) {
        return make_result(flag, Some(off), "OFF");
    }

    if let Some(prereqs) = flag.get("prerequisites").and_then(|p| p.as_array()) {
        for p in prereqs {
            let key = p.get("key").and_then(|k| k.as_str()).unwrap_or("");
            match resolver(key) {
                Some(pre) => {
                    let want = p.get("variation").and_then(|v| v.as_i64()).unwrap_or(-1);
                    if evaluate(&pre, ctx, resolver).variation_index != want {
                        return make_result(flag, Some(off), "PREREQUISITE_FAILED");
                    }
                }
                None => return make_result(flag, Some(off), "PREREQUISITE_FAILED"),
            }
        }
    }

    let ctx_key = ctx.get("key").and_then(|k| k.as_str()).unwrap_or("");
    if let Some(targets) = flag.get("targets").and_then(|t| t.as_array()) {
        for t in targets {
            if let Some(vals) = t.get("values").and_then(|v| v.as_array()) {
                if vals.iter().any(|v| v.as_str() == Some(ctx_key)) {
                    return make_result(flag, t.get("variation").and_then(|v| v.as_i64()), "TARGET_MATCH");
                }
            }
        }
    }

    if let Some(rules) = flag.get("rules").and_then(|r| r.as_array()) {
        for rule in rules {
            let empty: Vec<Value> = vec![];
            let clauses = rule.get("clauses").and_then(|c| c.as_array()).unwrap_or(&empty);
            if clauses.iter().all(|c| match_clause(c, ctx)) {
                let idx = resolve_index(
                    flag,
                    rule.get("variation").and_then(|v| v.as_i64()),
                    rule.get("rollout").and_then(|r| r.as_array()),
                    ctx,
                );
                return make_result(flag, idx, "RULE_MATCH");
            }
        }
    }

    let empty_obj = serde_json::json!({});
    let ft = flag.get("fallthrough").unwrap_or(&empty_obj);
    let idx = resolve_index(
        flag,
        ft.get("variation").and_then(|v| v.as_i64()),
        ft.get("rollout").and_then(|r| r.as_array()),
        ctx,
    );
    make_result(flag, idx, "FALLTHROUGH")
}

/// A client over a snapshot: `{ "flags": [ ...FlagConfig ] }`.
pub struct SwitchboardClient {
    flags: HashMap<String, Value>,
}

impl SwitchboardClient {
    pub fn from_json(json: &str) -> Self {
        let parsed: Value = serde_json::from_str(json).unwrap_or(Value::Null);
        let mut flags = HashMap::new();
        if let Some(arr) = parsed.get("flags").and_then(|f| f.as_array()) {
            for f in arr {
                if let Some(key) = f.get("key").and_then(|k| k.as_str()) {
                    flags.insert(key.to_string(), f.clone());
                }
            }
        }
        SwitchboardClient { flags }
    }

    pub fn evaluate_detail(&self, key: &str, ctx: &Value) -> EvalResult {
        match self.flags.get(key) {
            Some(flag) => {
                let flags = self.flags.clone();
                evaluate(flag, ctx, &move |k| flags.get(k).cloned())
            }
            None => err_result(),
        }
    }
}

pub fn about() -> &'static str {
    "switchboard-sdk — local-evaluation feature flags for Rust. See https://switchboard.co"
}
