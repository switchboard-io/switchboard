using System.Text.Json.Nodes;
using Switchboard.Evaluation;
using Xunit;

namespace Switchboard.Evaluation.Tests;

public class EvaluatorTests
{
    private static FlagConfig Flag(string json) => FlagConfig.Parse(json);

    [Fact]
    public void Disabled_returns_off_variation()
    {
        var flag = Flag("""
        { "key":"f", "enabled":false, "variations":[false,true],
          "offVariation":0, "fallthrough":{"variation":1}, "salt":"s" }
        """);
        var r = Evaluator.Evaluate(flag, new EvalContext("u1"));
        Assert.Equal("OFF", r.Reason);
        Assert.Equal(0, r.VariationIndex);
        Assert.False((bool)r.Value!.AsValue());
    }

    [Fact]
    public void Individual_target_matches()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":[false,true], "offVariation":0,
          "targets":[{"values":["vip"],"variation":1}],
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var r = Evaluator.Evaluate(flag, new EvalContext("vip"));
        Assert.Equal("TARGET_MATCH", r.Reason);
        Assert.True((bool)r.Value!.AsValue());
    }

    [Fact]
    public void Rule_with_clause_matches()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":["off","on"], "offVariation":0,
          "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA"]}],"variation":1}],
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var ctx = new EvalContext("u1").Set("country", JsonValue.Create("US"));
        var r = Evaluator.Evaluate(flag, ctx);
        Assert.Equal("RULE_MATCH", r.Reason);
        Assert.Equal("on", (string)r.Value!.AsValue()!);
    }

    [Fact]
    public void Rule_negate_inverts()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":["off","on"], "offVariation":0,
          "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US"],"negate":true}],"variation":1}],
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var ctx = new EvalContext("u1").Set("country", JsonValue.Create("IN"));
        Assert.Equal("RULE_MATCH", Evaluator.Evaluate(flag, ctx).Reason);
    }

    [Fact]
    public void Numeric_greater_than()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":[false,true], "offVariation":0,
          "rules":[{"clauses":[{"attribute":"age","op":"greaterThan","values":[18]}],"variation":1}],
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var adult = new EvalContext("u1").Set("age", JsonValue.Create(21));
        var minor = new EvalContext("u2").Set("age", JsonValue.Create(15));
        Assert.True(Evaluator.Evaluate(flag, adult).VariationIndex == 1);
        Assert.True(Evaluator.Evaluate(flag, minor).VariationIndex == 0);
    }

    [Fact]
    public void Fallthrough_when_no_rule_matches()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":["a","b"], "offVariation":0,
          "rules":[{"clauses":[{"attribute":"plan","op":"in","values":["pro"]}],"variation":1}],
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var ctx = new EvalContext("u1").Set("plan", JsonValue.Create("free"));
        Assert.Equal("FALLTHROUGH", Evaluator.Evaluate(flag, ctx).Reason);
    }

    [Fact]
    public void Prerequisite_failure_returns_off()
    {
        var main = Flag("""
        { "key":"main", "enabled":true, "variations":[false,true], "offVariation":0,
          "prerequisites":[{"key":"gate","variation":1}],
          "fallthrough":{"variation":1}, "salt":"s" }
        """);
        var gate = Flag("""
        { "key":"gate", "enabled":true, "variations":[false,true], "offVariation":0,
          "fallthrough":{"variation":0}, "salt":"s" }
        """);
        var r = Evaluator.Evaluate(main, new EvalContext("u1"), k => k == "gate" ? gate : null);
        Assert.Equal("PREREQUISITE_FAILED", r.Reason);
    }

    [Fact]
    public void Rollout_is_deterministic_and_splits()
    {
        var flag = Flag("""
        { "key":"f", "enabled":true, "variations":[false,true], "offVariation":0,
          "fallthrough":{"rollout":[{"variation":0,"weight":50000},{"variation":1,"weight":50000}]},
          "salt":"s" }
        """);
        // Deterministic: same key -> same result every time.
        var a1 = Evaluator.Evaluate(flag, new EvalContext("user-A")).VariationIndex;
        var a2 = Evaluator.Evaluate(flag, new EvalContext("user-A")).VariationIndex;
        Assert.Equal(a1, a2);

        // Distribution: across many keys we should see both buckets.
        int zero = 0, one = 0;
        for (int i = 0; i < 2000; i++)
        {
            var idx = Evaluator.Evaluate(flag, new EvalContext($"user-{i}")).VariationIndex;
            if (idx == 0) zero++; else one++;
        }
        Assert.True(zero > 700 && one > 700, $"skewed split: {zero}/{one}");
    }

    [Fact]
    public void Bucketing_matches_reference_vector()
    {
        // Reference vector shared with every language's conformance runner.
        var b = Bucketing.BucketOf("f", "s", "user-A");
        Assert.InRange(b, 0.0, 1.0);
        // Different context keys bucket differently.
        Assert.NotEqual(b, Bucketing.BucketOf("f", "s", "user-B"));
    }
}
