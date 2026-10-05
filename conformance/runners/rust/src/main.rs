// Conformance runner (Rust). Evaluates the shared corpus and writes results/rust.json.
use serde_json::{json, Value};
use std::fs;
use std::path::PathBuf;
use switchboard_sdk::{bucket_of, SwitchboardClient};

fn find_root() -> PathBuf {
    let mut dir = std::env::current_dir().unwrap();
    loop {
        if dir.join("corpus.json").exists() {
            return dir;
        }
        if dir.join("conformance/corpus.json").exists() {
            return dir.join("conformance");
        }
        if !dir.pop() {
            panic!("corpus.json not found");
        }
    }
}

fn main() {
    let root = find_root();
    let raw = fs::read_to_string(root.join("corpus.json")).unwrap();
    let corpus: Value = serde_json::from_str(&raw).unwrap();

    let snapshot = json!({ "flags": corpus["flags"] }).to_string();
    let client = SwitchboardClient::from_json(&snapshot);

    let mut cases = Vec::new();
    for c in corpus["cases"].as_array().unwrap() {
        let r = client.evaluate_detail(c["flag"].as_str().unwrap(), &c["context"]);
        cases.push(json!({
            "id": c["id"],
            "variationIndex": r.variation_index,
            "reason": r.reason,
            "value": r.value,
        }));
    }

    let mut buckets = Vec::new();
    for b in corpus["buckets"].as_array().unwrap() {
        let fk = b["flagKey"].as_str().unwrap();
        let salt = b["salt"].as_str().unwrap();
        let ck = b["contextKey"].as_str().unwrap();
        buckets.push(json!({
            "id": format!("{}.{}.{}", fk, salt, ck),
            "actual": bucket_of(fk, salt, ck),
        }));
    }

    let results_dir = root.join("results");
    fs::create_dir_all(&results_dir).unwrap();
    let out = json!({ "lang": "rust", "cases": cases, "buckets": buckets });
    fs::write(results_dir.join("rust.json"), serde_json::to_string_pretty(&out).unwrap()).unwrap();
    println!("rust runner: {} cases, {} buckets", cases.len(), buckets.len());
}
