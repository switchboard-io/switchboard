using System.Text.Json.Nodes;
using Switchboard;
using Switchboard.Evaluation;

namespace Switchboard.OpenFeature;

/// <summary>
/// OpenFeature-style provider backed by the Switchboard local-evaluation client.
/// Exposes resolve methods that return a value plus the evaluation reason, mirroring
/// OpenFeature's ResolutionDetails shape. A thin adapter can bind these to the
/// OpenFeature FeatureProvider interface.
/// </summary>
public sealed class SwitchboardProvider
{
    public string Metadata => "Switchboard";

    private readonly SwitchboardClient _client;

    public SwitchboardProvider(SwitchboardClient client) => _client = client;

    public static SwitchboardProvider FromJson(string snapshot) =>
        new SwitchboardProvider(SwitchboardClient.FromJson(snapshot));

    public (bool value, string reason) ResolveBoolean(string key, EvalContext ctx, bool def = false)
    {
        var r = _client.EvaluateDetail(key, ctx);
        var val = r.Value is JsonValue v && v.TryGetValue<bool>(out var b) ? b : def;
        return (val, r.Reason);
    }

    public (string value, string reason) ResolveString(string key, EvalContext ctx, string def = "")
    {
        var r = _client.EvaluateDetail(key, ctx);
        var val = r.Value is JsonValue v && v.TryGetValue<string>(out var s) ? s : def;
        return (val, r.Reason);
    }

    public (double value, string reason) ResolveNumber(string key, EvalContext ctx, double def = 0)
    {
        var r = _client.EvaluateDetail(key, ctx);
        var val = r.Value is JsonValue v && v.TryGetValue<double>(out var d) ? d : def;
        return (val, r.Reason);
    }
}