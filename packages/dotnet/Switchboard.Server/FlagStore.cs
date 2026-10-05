using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Switchboard.Evaluation;

namespace Switchboard.Server;

/// <summary>
/// In-memory flag store + change broadcaster. This is the control-plane's source of
/// truth for the demo/all-in-one deployment; a production build swaps this for
/// PostgreSQL + Redis fan-out behind the same interface.
/// </summary>
public sealed class FlagStore
{
    private readonly ConcurrentDictionary<string, FlagConfig> _flags = new();
    private readonly List<Channel> _subscribers = new();
    private readonly object _lock = new();
    private long _version;

    public long Version => Interlocked.Read(ref _version);

    public FlagStore()
    {
        // Seed a demo flag so the server is useful on first run.
        var demo = FlagConfig.Parse("""
        { "key":"welcome-banner", "enabled":true, "variations":[false,true], "offVariation":0,
          "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA"]}],"variation":1}],
          "fallthrough":{"rollout":[{"variation":0,"weight":50000},{"variation":1,"weight":50000}]},
          "salt":"seed" }
        """);
        _flags[demo.Key] = demo;
    }

    public IReadOnlyCollection<FlagConfig> All() => _flags.Values.ToList();

    public FlagConfig? Get(string key) => _flags.TryGetValue(key, out var f) ? f : null;

    public FlagConfig? Resolver(string key) => Get(key);

    public void Upsert(FlagConfig flag)
    {
        _flags[flag.Key] = flag;
        Bump();
    }

    public bool Delete(string key)
    {
        var ok = _flags.TryRemove(key, out _);
        if (ok) Bump();
        return ok;
    }

    public string Snapshot()
    {
        var arr = new JsonArray();
        foreach (var f in _flags.Values)
            arr.Add(JsonNode.Parse(JsonSerializer.Serialize(f, SerializerOptions)));
        return new JsonObject { ["version"] = Version, ["flags"] = arr }.ToJsonString();
    }

    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    // --- change streaming (SSE) ---
    private sealed class Channel
    {
        public readonly System.Threading.Channels.Channel<string> Ch =
            System.Threading.Channels.Channel.CreateUnbounded<string>();
    }

    public (IAsyncEnumerable<string> stream, IDisposable sub) Subscribe()
    {
        var channel = new Channel();
        lock (_lock) _subscribers.Add(channel);
        var disposable = new Unsubscriber(() => { lock (_lock) _subscribers.Remove(channel); });
        return (ReadAll(channel), disposable);
    }

    private static async IAsyncEnumerable<string> ReadAll(Channel c)
    {
        while (await c.Ch.Reader.WaitToReadAsync())
            while (c.Ch.Reader.TryRead(out var msg))
                yield return msg;
    }

    private void Bump()
    {
        var v = Interlocked.Increment(ref _version);
        var payload = new JsonObject { ["version"] = v }.ToJsonString();
        lock (_lock)
            foreach (var s in _subscribers)
                s.Ch.Writer.TryWrite(payload);
    }

    private sealed class Unsubscriber : IDisposable
    {
        private readonly Action _dispose;
        public Unsubscriber(Action dispose) => _dispose = dispose;
        public void Dispose() => _dispose();
    }
}
