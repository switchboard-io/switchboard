using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Switchboard.Evaluation;

/// <summary>A feature flag's configuration (see docs/SPEC.md).</summary>
public sealed class FlagConfig
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("variations")] public List<JsonNode?> Variations { get; set; } = new();
    [JsonPropertyName("offVariation")] public int OffVariation { get; set; }
    [JsonPropertyName("fallthrough")] public VariationOrRollout Fallthrough { get; set; } = new();
    [JsonPropertyName("targets")] public List<Target> Targets { get; set; } = new();
    [JsonPropertyName("rules")] public List<Rule> Rules { get; set; } = new();
    [JsonPropertyName("prerequisites")] public List<Prerequisite> Prerequisites { get; set; } = new();
    [JsonPropertyName("salt")] public string Salt { get; set; } = "";

    public static FlagConfig Parse(string json) =>
        JsonSerializer.Deserialize<FlagConfig>(json, Json.Options)!;
}

public sealed class VariationOrRollout
{
    [JsonPropertyName("variation")] public int? Variation { get; set; }
    [JsonPropertyName("rollout")] public List<WeightedVariation>? Rollout { get; set; }
}

public sealed class WeightedVariation
{
    [JsonPropertyName("variation")] public int Variation { get; set; }
    [JsonPropertyName("weight")] public int Weight { get; set; }
}

public sealed class Target
{
    [JsonPropertyName("values")] public List<string> Values { get; set; } = new();
    [JsonPropertyName("variation")] public int Variation { get; set; }
}

public sealed class Rule
{
    [JsonPropertyName("clauses")] public List<Clause> Clauses { get; set; } = new();
    [JsonPropertyName("variation")] public int? Variation { get; set; }
    [JsonPropertyName("rollout")] public List<WeightedVariation>? Rollout { get; set; }
}

public sealed class Clause
{
    [JsonPropertyName("attribute")] public string Attribute { get; set; } = "";
    [JsonPropertyName("op")] public string Op { get; set; } = "";
    [JsonPropertyName("values")] public List<JsonNode?> Values { get; set; } = new();
    [JsonPropertyName("negate")] public bool Negate { get; set; }
}

public sealed class Prerequisite
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("variation")] public int Variation { get; set; }
}

/// <summary>Context describing who/what a flag is evaluated for.</summary>
public sealed class EvalContext
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("attributes")] public Dictionary<string, JsonNode?> Attributes { get; set; } = new();

    public EvalContext() { }
    public EvalContext(string key) { Key = key; }

    public EvalContext Set(string name, JsonNode? value) { Attributes[name] = value; return this; }
}

/// <summary>The result of an evaluation.</summary>
public sealed class EvalResult
{
    public JsonNode? Value { get; }
    public int VariationIndex { get; }
    public string Reason { get; }

    public EvalResult(JsonNode? value, int variationIndex, string reason)
    {
        Value = value; VariationIndex = variationIndex; Reason = reason;
    }
}

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
