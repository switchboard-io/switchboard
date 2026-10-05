namespace Switchboard;

/// <summary>
/// Placeholder entry point for the Switchboard feature-flag SDK.
/// This 0.0.1 release exists to reserve the <c>Switchboard.Sdk</c> package ID on NuGet.org.
/// The full local-evaluation, streaming, OpenFeature-compatible client ships in a later version.
/// </summary>
public static class SwitchboardClient
{
    /// <summary>The product name.</summary>
    public const string Name = "Switchboard";

    /// <summary>Marker indicating this is a placeholder reservation build.</summary>
    public const bool IsPlaceholder = true;

    /// <summary>Returns a short description of the SDK.</summary>
    public static string About() =>
        "Switchboard.Sdk (placeholder) — local-evaluation feature flags for .NET. See https://switchboard.co";
}
