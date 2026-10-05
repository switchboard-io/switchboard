namespace Switchboard.Cli;

/// <summary>
/// Placeholder CLI entry point. Reserves the <c>Switchboard.Cli</c> tool package
/// and the <c>switchboard</c> command name on NuGet.org.
/// </summary>
internal static class Program
{
    private const string Version = "0.0.1";

    private static int Main(string[] args)
    {
        Console.WriteLine($"Switchboard CLI v{Version} (placeholder)");
        Console.WriteLine("Feature management from your terminal — https://switchboard.co");
        Console.WriteLine();
        Console.WriteLine("This is a placeholder release that reserves the 'switchboard' command.");
        Console.WriteLine("Planned commands:");
        Console.WriteLine("  switchboard login    --url <server>");
        Console.WriteLine("  switchboard flag     create|on|off|rollout <key>");
        Console.WriteLine("  switchboard eval     <key> --context '{\"key\":\"user-1\"}'");
        Console.WriteLine("  switchboard export | import <file>");
        Console.WriteLine("  switchboard lifecycle scan|stale");
        return 0;
    }
}
