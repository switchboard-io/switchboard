using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Switchboard;
using Switchboard.Evaluation;

namespace Switchboard.Cli;

internal static class Program
{
    private const string Version = "0.0.1";

    private static int Main(string[] args)
    {
        if (args.Length == 0) { PrintHelp(); return 0; }

        switch (args[0])
        {
            case "eval": return CmdEval(args);
            case "bucket": return CmdBucket(args);
            case "--version": Console.WriteLine(Version); return 0;
            case "-h": case "--help": case "help": PrintHelp(); return 0;
            default:
                Console.Error.WriteLine($"Unknown command: {args[0]}");
                PrintHelp();
                return 1;
        }
    }

    // switchboard eval <flagKey> --config <file.json> --context '{"key":"u1"}'
    private static int CmdEval(string[] args)
    {
        var flagKey = args.Length > 1 ? args[1] : null;
        var configPath = GetOpt(args, "--config");
        var contextJson = GetOpt(args, "--context") ?? "{\"key\":\"anonymous\"}";
        if (flagKey == null || configPath == null)
        {
            Console.Error.WriteLine("usage: switchboard eval <flagKey> --config <file.json> --context '{\"key\":\"u1\"}'");
            return 1;
        }

        var snapshot = File.ReadAllText(configPath);
        var client = SwitchboardClient.FromJson(snapshot);
        var ctx = ParseContext(contextJson);
        var r = client.EvaluateDetail(flagKey, ctx);

        var outp = new JsonObject
        {
            ["flag"] = flagKey,
            ["value"] = r.Value?.DeepClone(),
            ["variationIndex"] = r.VariationIndex,
            ["reason"] = r.Reason,
        };
        Console.WriteLine(outp.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return r.Reason == "ERROR" ? 2 : 0;
    }

    // switchboard bucket <flagKey> <salt> <contextKey>
    private static int CmdBucket(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("usage: switchboard bucket <flagKey> <salt> <contextKey>");
            return 1;
        }
        var b = Bucketing.BucketOf(args[1], args[2], args[3]);
        Console.WriteLine(b.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        return 0;
    }

    private static EvalContext ParseContext(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var ctx = new EvalContext();
        if (doc.RootElement.TryGetProperty("key", out var k)) ctx.Key = k.GetString() ?? "";
        if (doc.RootElement.TryGetProperty("attributes", out var attrs))
            foreach (var p in attrs.EnumerateObject())
                ctx.Attributes[p.Name] = JsonNode.Parse(p.Value.GetRawText());
        return ctx;
    }

    private static string? GetOpt(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine($"Switchboard CLI v{Version} — feature management from your terminal");
        Console.WriteLine("https://switchboard.co");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  eval <flagKey> --config <file.json> --context '{\"key\":\"u1\"}'");
        Console.WriteLine("        Evaluate a flag against a context (local, using the engine).");
        Console.WriteLine("  bucket <flagKey> <salt> <contextKey>");
        Console.WriteLine("        Print the deterministic rollout bucket in [0,1).");
        Console.WriteLine("  --version");
    }
}