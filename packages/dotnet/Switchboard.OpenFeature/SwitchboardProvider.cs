namespace Switchboard.OpenFeature;

/// <summary>
/// Placeholder OpenFeature provider for Switchboard (.NET).
/// Reserves the <c>Switchboard.OpenFeature</c> package ID on NuGet.org while the
/// full provider (implementing OpenFeature's FeatureProvider) is built.
/// Docs: https://switchboard.co
/// </summary>
public static class SwitchboardProvider
{
    /// <summary>Provider metadata name reported to OpenFeature.</summary>
    public const string Name = "Switchboard";

    /// <summary>Marks this as a placeholder reservation build.</summary>
    public const bool IsPlaceholder = true;

    /// <summary>Returns a short description of the provider.</summary>
    public static string About() =>
        "Switchboard.OpenFeature (placeholder) — OpenFeature provider for .NET. See https://switchboard.co";
}
