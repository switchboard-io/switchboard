using System.Text.Json;
using System.Text.Json.Nodes;
using Switchboard.Evaluation;

// Conformance runner (.NET). Evaluates the shared corpus and writes results/dotnet.json.

var here = AppContext.BaseDirectory;
// walk up to the conformance/ directory
var dir = new DirectoryInfo(here);
while (dir != null && !File.Exists(Path.Combine(dir.FullName, "corpus.json")))
    dir = dir.Parent;
if (dir == null) { Console.Error.WriteLine("corpus.json not found"); return 1; }
var root = dir.FullName;

var corpus = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "corpus.json")))!.AsObject();

var flags = new Dictionary<string, FlagConfig>();
foreach (var f in corpus["flags"]!.AsArray())
{
    var flag = FlagConfig.Parse(f!.ToJsonString());
    flags[flag.Key] = flag;
}
FlagConfig? Resolver(string k) => flags.TryGetValue(k, out var f) ? f : null;

EvalContext ParseCtx(JsonNode node)
{
    var ctx = new EvalContext();
    var o = node.AsObject();
    if (o.TryGetPropertyValue("key", out var k)) ctx.Key = k!.GetValue<string>();
    if (o.TryGetPropertyValue("attributes", out var a) && a is JsonObject attrs)
        foreach (var p in attrs) ctx.Attributes[p.Key] = p.Value?.DeepClone();
    return ctx;
}

var cases = new JsonArray();
foreach (var c in corpus["cases"]!.AsArray())
{
    var co = c!.AsObject();
    var flagKey = co["flag"]!.GetValue<string>();
    var ctx = ParseCtx(co["context"]!);
    var flag = flags[flagKey];
    var r = Evaluator.Evaluate(flag, ctx, Resolver);
    cases.Add(new JsonObject
    {
        ["id"] = co["id"]!.GetValue<string>(),
        ["variationIndex"] = r.VariationIndex,
        ["reason"] = r.Reason,
        ["value"] = r.Value?.DeepClone(),
    });
}

var buckets = new JsonArray();
foreach (var b in corpus["buckets"]!.AsArray())
{
    var bo = b!.AsObject();
    var fk = bo["flagKey"]!.GetValue<string>();
    var salt = bo["salt"]!.GetValue<string>();
    var ck = bo["contextKey"]!.GetValue<string>();
    buckets.Add(new JsonObject { ["id"] = $"{fk}.{salt}.{ck}", ["actual"] = Bucketing.BucketOf(fk, salt, ck) });
}

var resultsDir = Path.Combine(root, "results");
Directory.CreateDirectory(resultsDir);
var outObj = new JsonObject { ["lang"] = "dotnet", ["cases"] = cases, ["buckets"] = buckets };
File.WriteAllText(Path.Combine(resultsDir, "dotnet.json"), outObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"dotnet runner: {cases.Count} cases, {buckets.Count} buckets");
return 0;
