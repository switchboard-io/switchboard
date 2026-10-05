using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Switchboard.Evaluation;

/// <summary>
/// The deterministic feature-flag evaluation engine. Pure and side-effect free:
/// given a flag config and a context it returns a value, variation index, and reason.
/// Implements docs/SPEC.md and is byte-for-byte compatible with the other language SDKs.
/// </summary>
public static class Evaluator
{
    /// <summary>Resolves a prerequisite flag by key (returns null if unknown).</summary>
    public delegate FlagConfig? PrerequisiteResolver(string key);

    public static EvalResult Evaluate(
        FlagConfig flag,
        EvalContext context,
        PrerequisiteResolver? resolver = null)
    {
        try
        {
            if (!flag.Enabled)
                return Variation(flag, flag.OffVariation, "OFF");

            foreach (var p in flag.Prerequisites)
            {
                var pre = resolver?.Invoke(p.Key);
                if (pre == null)
                    return Variation(flag, flag.OffVariation, "PREREQUISITE_FAILED");
                var preResult = Evaluate(pre, context, resolver);
                if (preResult.VariationIndex != p.Variation)
                    return Variation(flag, flag.OffVariation, "PREREQUISITE_FAILED");
            }

            foreach (var t in flag.Targets)
                if (t.Values.Contains(context.Key))
                    return Variation(flag, t.Variation, "TARGET_MATCH");

            foreach (var rule in flag.Rules)
            {
                var all = true;
                foreach (var clause in rule.Clauses)
                    if (!MatchClause(clause, context)) { all = false; break; }
                if (all)
                {
                    var idx = ResolveIndex(flag, rule.Variation, rule.Rollout, context);
                    return Variation(flag, idx, "RULE_MATCH");
                }
            }

            var fIdx = ResolveIndex(flag, flag.Fallthrough.Variation, flag.Fallthrough.Rollout, context);
            return Variation(flag, fIdx, "FALLTHROUGH");
        }
        catch
        {
            return new EvalResult(null, -1, "ERROR");
        }
    }

    private static EvalResult Variation(FlagConfig flag, int index, string reason)
    {
        if (index < 0 || index >= flag.Variations.Count)
            return new EvalResult(null, -1, "ERROR");
        var v = flag.Variations[index];
        return new EvalResult(v?.DeepClone(), index, reason);
    }

    private static int ResolveIndex(FlagConfig flag, int? variation, List<WeightedVariation>? rollout, EvalContext ctx)
    {
        if (variation.HasValue) return variation.Value;
        if (rollout != null && rollout.Count > 0)
        {
            var bucket = Bucketing.BucketOf(flag.Key, flag.Salt, ctx.Key);
            double cumulative = 0;
            foreach (var wv in rollout)
            {
                cumulative += wv.Weight;
                if (bucket * 100000 < cumulative) return wv.Variation;
            }
            return rollout[rollout.Count - 1].Variation;
        }
        throw new InvalidOperationException("rule has neither variation nor rollout");
    }

    private static bool MatchClause(Clause clause, EvalContext ctx)
    {
        var attrVal = clause.Attribute == "key"
            ? JsonValue.Create(ctx.Key)
            : (ctx.Attributes.TryGetValue(clause.Attribute, out var v) ? v : null);

        var result = EvalOp(clause.Op, attrVal, clause.Values);
        return clause.Negate ? !result : result;
    }

    private static bool EvalOp(string op, JsonNode? attr, List<JsonNode?> values)
    {
        if (attr == null) return false;

        switch (op)
        {
            case "in":
                foreach (var val in values)
                    if (ScalarEquals(attr, val)) return true;
                return false;
            case "contains":
                return AsString(attr) is string s0 && AnyString(values, t => s0.Contains(t));
            case "startsWith":
                return AsString(attr) is string s1 && AnyString(values, t => s1.StartsWith(t, StringComparison.Ordinal));
            case "endsWith":
                return AsString(attr) is string s2 && AnyString(values, t => s2.EndsWith(t, StringComparison.Ordinal));
            case "greaterThan":
                return AsNumber(attr) is double a0 && AsNumber(First(values)) is double b0 && a0 > b0;
            case "lessThan":
                return AsNumber(attr) is double a1 && AsNumber(First(values)) is double b1 && a1 < b1;
            case "regexMatch":
                return AsString(attr) is string s3 && AsString(First(values)) is string pat
                       && Regex.IsMatch(s3, pat);
            default:
                return false;
        }
    }

    private static JsonNode? First(List<JsonNode?> values) => values.Count > 0 ? values[0] : null;

    private static bool AnyString(List<JsonNode?> values, Func<string, bool> pred)
    {
        foreach (var val in values)
            if (AsString(val) is string s && pred(s)) return true;
        return false;
    }

    private static bool ScalarEquals(JsonNode? a, JsonNode? b)
    {
        if (a == null || b == null) return false;
        var an = AsNumber(a); var bn = AsNumber(b);
        if (an.HasValue && bn.HasValue) return an.Value == bn.Value;
        var ab = AsBool(a); var bb = AsBool(b);
        if (ab.HasValue && bb.HasValue) return ab.Value == bb.Value;
        return AsString(a) == AsString(b);
    }

    private static string? AsString(JsonNode? n)
    {
        if (n is JsonValue jv)
        {
            if (jv.TryGetValue<string>(out var s)) return s;
            if (jv.TryGetValue<double>(out var d)) return d.ToString(CultureInfo.InvariantCulture);
            if (jv.TryGetValue<bool>(out var b)) return b ? "true" : "false";
        }
        return null;
    }

    private static double? AsNumber(JsonNode? n)
    {
        if (n is JsonValue jv)
        {
            if (jv.TryGetValue<double>(out var d)) return d;
            if (jv.TryGetValue<int>(out var i)) return i;
            if (jv.TryGetValue<long>(out var l)) return l;
        }
        return null;
    }

    private static bool? AsBool(JsonNode? n)
        => n is JsonValue jv && jv.TryGetValue<bool>(out var b) ? b : (bool?)null;
}
