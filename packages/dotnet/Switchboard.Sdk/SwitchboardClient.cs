using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Switchboard.Evaluation;

namespace Switchboard;

/// <summary>
/// A local-evaluation feature-flag client. Holds a set of flag configurations and
/// evaluates them in-process (no network call per evaluation) using the shared
/// Switchboard evaluation engine. Flags are supplied as a JSON snapshot — in a full
/// deployment the streaming client keeps this snapshot fresh from the server.
/// </summary>
public sealed class SwitchboardClient
{
    private readonly Dictionary<string, FlagConfig> _flags;

    public SwitchboardClient(IEnumerable<FlagConfig> flags)
    {
        _flags = new Dictionary<string, FlagConfig>();
        foreach (var f in flags) _flags[f.Key] = f;
    }

    /// <summary>Builds a client from a JSON snapshot: { "flags": [ ...FlagConfig... ] }.</summary>
    public static SwitchboardClient FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var list = new List<FlagConfig>();
        if (doc.RootElement.TryGetProperty("flags", out var flagsEl))
            foreach (var el in flagsEl.EnumerateArray())
                list.Add(FlagConfig.Parse(el.GetRawText()));
        return new SwitchboardClient(list);
    }

    private EvalResult Eval(string key, EvalContext ctx)
    {
        if (!_flags.TryGetValue(key, out var flag))
            return new EvalResult(null, -1, "ERROR");
        return Evaluator.Evaluate(flag, ctx, k => _flags.TryGetValue(k, out var f) ? f : null);
    }

    /// <summary>Full evaluation detail (value, variation index, reason).</summary>
    public EvalResult EvaluateDetail(string key, EvalContext ctx) => Eval(key, ctx);

    public bool GetBool(string key, EvalContext ctx, bool defaultValue = false)
    {
        var r = Eval(key, ctx);
        return r.Value is JsonValue v && v.TryGetValue<bool>(out var b) ? b : defaultValue;
    }

    public string GetString(string key, EvalContext ctx, string defaultValue = "")
    {
        var r = Eval(key, ctx);
        return r.Value is JsonValue v && v.TryGetValue<string>(out var s) ? s : defaultValue;
    }

    public double GetNumber(string key, EvalContext ctx, double defaultValue = 0)
    {
        var r = Eval(key, ctx);
        return r.Value is JsonValue v && v.TryGetValue<double>(out var d) ? d : defaultValue;
    }

    /// <summary>Returns the raw JSON variation value (for JSON-kind flags).</summary>
    public JsonNode? GetJson(string key, EvalContext ctx) => Eval(key, ctx).Value;
}