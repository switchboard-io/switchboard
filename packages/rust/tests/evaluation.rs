use serde_json::json;
use switchboard_sdk::{bucket_of, SwitchboardClient};

fn client() -> SwitchboardClient {
    SwitchboardClient::from_json(
        r#"{ "flags": [
      { "key":"disabled", "enabled":false, "variations":[false,true], "offVariation":0, "fallthrough":{"variation":1}, "salt":"s" },
      { "key":"target", "enabled":true, "variations":[false,true], "offVariation":0, "targets":[{"values":["vip"],"variation":1}], "fallthrough":{"variation":0}, "salt":"s" },
      { "key":"rule", "enabled":true, "variations":["off","on"], "offVariation":0, "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA"]}],"variation":1}], "fallthrough":{"variation":0}, "salt":"s" },
      { "key":"rollout", "enabled":true, "variations":[false,true], "offVariation":0, "fallthrough":{"rollout":[{"variation":0,"weight":50000},{"variation":1,"weight":50000}]}, "salt":"s" }
    ] }"#,
    )
}

#[test]
fn bucket_parity() {
    assert_eq!(bucket_of("f", "s", "user-A"), 0.250274217889144);
    assert_eq!(bucket_of("checkout-v2", "abc", "user-1"), 0.5864417850195552);
}

#[test]
fn disabled_off() {
    let r = client().evaluate_detail("disabled", &json!({ "key": "u" }));
    assert_eq!(r.reason, "OFF");
    assert_eq!(r.value, json!(false));
}

#[test]
fn target_match() {
    let r = client().evaluate_detail("target", &json!({ "key": "vip" }));
    assert_eq!(r.reason, "TARGET_MATCH");
    assert_eq!(r.value, json!(true));
}

#[test]
fn rule_match() {
    let r = client().evaluate_detail("rule", &json!({ "key": "u", "attributes": { "country": "US" } }));
    assert_eq!(r.reason, "RULE_MATCH");
    assert_eq!(r.value, json!("on"));
}

#[test]
fn fallthrough() {
    let r = client().evaluate_detail("rule", &json!({ "key": "u", "attributes": { "country": "IN" } }));
    assert_eq!(r.reason, "FALLTHROUGH");
}

#[test]
fn rollout_deterministic() {
    let c = client();
    let a = c.evaluate_detail("rollout", &json!({ "key": "user-A" })).variation_index;
    let b = c.evaluate_detail("rollout", &json!({ "key": "user-A" })).variation_index;
    assert_eq!(a, b);
}
