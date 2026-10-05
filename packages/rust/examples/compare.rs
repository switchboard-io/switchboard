// Benchmark: evaluations/sec for switchboard-sdk (Rust). Run: cargo run --release --example compare
use serde_json::json;
use std::time::Instant;
use switchboard_sdk::SwitchboardClient;

fn main() {
    let client = SwitchboardClient::from_json(
        r#"{ "flags": [{
      "key":"checkout-v2", "enabled":true, "variations":["off","on"], "offVariation":0,
      "targets":[{"values":["vip"],"variation":1}],
      "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA","GB"]}],"variation":1}],
      "fallthrough":{"rollout":[{"variation":0,"weight":75000},{"variation":1,"weight":25000}]},
      "salt":"abc" }] }"#,
    );

    let ctx = |i: i64| json!({ "key": format!("user-{}", i), "attributes": { "country": if i % 2 == 0 { "US" } else { "IN" } } });

    for i in 0..100_000 {
        client.evaluate_detail("checkout-v2", &ctx(i));
    }

    let n: i64 = 2_000_000;
    let start = Instant::now();
    let mut sink: i64 = 0;
    for i in 0..n {
        sink += client.evaluate_detail("checkout-v2", &ctx(i)).variation_index;
    }
    let elapsed = start.elapsed();

    println!("Switchboard SDK benchmark (Rust)");
    println!("  evaluations : {}", n);
    println!("  total time  : {:.1} ms", elapsed.as_secs_f64() * 1000.0);
    println!("  per eval    : {:.1} ns", elapsed.as_nanos() as f64 / n as f64);
    println!("  throughput  : {:.0} evals/sec", n as f64 / elapsed.as_secs_f64());
    println!("  (sink={})", sink);
}
