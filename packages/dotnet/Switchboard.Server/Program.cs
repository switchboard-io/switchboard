using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Switchboard.Evaluation;
using Switchboard.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FlagStore>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var store = app.Services.GetRequiredService<FlagStore>();
var jsonOpts = FlagStore.SerializerOptions;

// --- Health ---
app.MapGet("/api/health", () => Results.Json(new { status = "ok", version = store.Version }));

// --- List all flags ---
app.MapGet("/api/flags", () => Results.Text(store.Snapshot(), "application/json"));

// --- Get one flag ---
app.MapGet("/api/flags/{key}", (string key) =>
{
    var f = store.Get(key);
    return f is null
        ? Results.NotFound(new { error = $"flag '{key}' not found" })
        : Results.Text(JsonSerializer.Serialize(f, jsonOpts), "application/json");
});

// --- Create/update a flag ---
app.MapPut("/api/flags/{key}", async (string key, HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    FlagConfig flag;
    try { flag = FlagConfig.Parse(body); }
    catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
    flag.Key = key; // path wins
    store.Upsert(flag);
    return Results.Json(new { ok = true, key, version = store.Version });
});

// --- Delete a flag ---
app.MapDelete("/api/flags/{key}", (string key) =>
    store.Delete(key)
        ? Results.Json(new { ok = true, version = store.Version })
        : Results.NotFound(new { error = $"flag '{key}' not found" }));

// --- Evaluate a flag server-side ---
// POST /api/eval/{key}  body: { "key":"user-1", "attributes": {...} }
app.MapPost("/api/eval/{key}", async (string key, HttpRequest req) =>
{
    var flag = store.Get(key);
    if (flag is null) return Results.NotFound(new { error = $"flag '{key}' not found" });

    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    EvalContext ctx;
    try
    {
        ctx = string.IsNullOrWhiteSpace(body)
            ? new EvalContext("anonymous")
            : ParseContext(body);
    }
    catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }

    var r = Evaluator.Evaluate(flag, ctx, store.Resolver);
    return Results.Json(new
    {
        flag = key,
        value = r.Value,
        variationIndex = r.VariationIndex,
        reason = r.Reason,
    });
});

// --- Server-Sent Events: push a notification whenever flags change ---
app.MapGet("/api/stream", async (HttpContext http) =>
{
    http.Response.Headers.Append("Content-Type", "text/event-stream");
    http.Response.Headers.Append("Cache-Control", "no-cache");
    var (stream, sub) = store.Subscribe();
    using (sub)
    {
        // initial hello with current version
        await http.Response.WriteAsync($"event: hello\ndata: {{\"version\":{store.Version}}}\n\n");
        await http.Response.Body.FlushAsync();
        try
        {
            await foreach (var msg in stream.WithCancellation(http.RequestAborted))
            {
                await http.Response.WriteAsync($"event: change\ndata: {msg}\n\n");
                await http.Response.Body.FlushAsync();
            }
        }
        catch (OperationCanceledException) { /* client disconnected */ }
    }
});

app.Run();

static EvalContext ParseContext(string json)
{
    using var doc = JsonDocument.Parse(json);
    var ctx = new EvalContext();
    if (doc.RootElement.TryGetProperty("key", out var k)) ctx.Key = k.GetString() ?? "";
    if (doc.RootElement.TryGetProperty("attributes", out var attrs))
        foreach (var p in attrs.EnumerateObject())
            ctx.Attributes[p.Name] = JsonNode.Parse(p.Value.GetRawText());
    return ctx;
}

public partial class Program { }
