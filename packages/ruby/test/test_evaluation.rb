# frozen_string_literal: true

require "minitest/autorun"
require "switchboard/sdk"

class EvaluationTest < Minitest::Test
  SNAPSHOT = {
    "flags" => [
      { "key" => "disabled", "enabled" => false, "variations" => [false, true], "offVariation" => 0, "fallthrough" => { "variation" => 1 }, "salt" => "s" },
      { "key" => "target", "enabled" => true, "variations" => [false, true], "offVariation" => 0, "targets" => [{ "values" => ["vip"], "variation" => 1 }], "fallthrough" => { "variation" => 0 }, "salt" => "s" },
      { "key" => "rule", "enabled" => true, "variations" => %w[off on], "offVariation" => 0, "rules" => [{ "clauses" => [{ "attribute" => "country", "op" => "in", "values" => %w[US CA] }], "variation" => 1 }], "fallthrough" => { "variation" => 0 }, "salt" => "s" },
      { "key" => "rollout", "enabled" => true, "variations" => [false, true], "offVariation" => 0, "fallthrough" => { "rollout" => [{ "variation" => 0, "weight" => 50_000 }, { "variation" => 1, "weight" => 50_000 }] }, "salt" => "s" }
    ]
  }.freeze

  def setup
    @client = Switchboard::Sdk::Client.from_json(SNAPSHOT)
  end

  def test_bucket_parity
    assert_equal 0.250274217889144, Switchboard::Sdk.bucket_of("f", "s", "user-A")
    assert_equal 0.5864417850195552, Switchboard::Sdk.bucket_of("checkout-v2", "abc", "user-1")
  end

  def test_disabled
    r = @client.evaluate_detail("disabled", { "key" => "u" })
    assert_equal "OFF", r["reason"]
    assert_equal false, r["value"]
  end

  def test_target
    r = @client.evaluate_detail("target", { "key" => "vip" })
    assert_equal "TARGET_MATCH", r["reason"]
    assert_equal true, r["value"]
  end

  def test_rule
    r = @client.evaluate_detail("rule", { "key" => "u", "attributes" => { "country" => "US" } })
    assert_equal "RULE_MATCH", r["reason"]
    assert_equal "on", r["value"]
  end

  def test_fallthrough
    r = @client.evaluate_detail("rule", { "key" => "u", "attributes" => { "country" => "IN" } })
    assert_equal "FALLTHROUGH", r["reason"]
  end

  def test_rollout_deterministic
    a = @client.evaluate_detail("rollout", { "key" => "user-A" })["variationIndex"]
    b = @client.evaluate_detail("rollout", { "key" => "user-A" })["variationIndex"]
    assert_equal a, b
  end
end
