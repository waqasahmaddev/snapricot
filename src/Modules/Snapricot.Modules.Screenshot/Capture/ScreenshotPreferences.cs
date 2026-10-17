using Snapricot.Core.Settings;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>
/// Reads and updates <see cref="ScreenshotSettings"/>, and decides where captures go.
/// Saving rules: no folder chosen = <c>Pictures\Snapricot</c>; a folder chosen with
/// "Always save here" becomes the default; a default that no longer exists (unplugged drive) falls back to <c>Pictures\Snapricot</c>.
/// </summary>
public sealed class ScreenshotPreferences(ISettingsStore store, string fallbackFolder)
{
    public const string Section = "screenshot";

    public ScreenshotPreferences(ISettingsStore store)
        : this(store, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Snapricot"))
    {
    }

    /// <summary>Raised after any change, so open Settings views and overlays stay in sync.</summary>
    public event Action? Changed;

    public string FallbackFolder { get; } = fallbackFolder;

    public ScreenshotSettings Settings => store.Load(Section, ScreenshotSettingsJson.Default.ScreenshotSettings, () => new ScreenshotSettings());

    /// <summary>The folder Copy auto-saves to.</summary>
    public string DefaultFolder => Usable(Settings.DefaultFolder) ?? FallbackFolder;

    /// <summary>True when the user picked a default that is currently unavailable.</summary>
    public bool DefaultIsMissing => Settings.DefaultFolder is { } folder && Usable(folder) is null;

    /// <summary>The folder the Save dialog opens with: the last one used if it still exists, else the default.</summary>
    public string SuggestedFolder => Usable(Settings.LastFolder) ?? DefaultFolder;

    public string FileNamePattern => Settings.FileNamePattern;

    public bool IsDefault(string folder) => SameFolder(folder, DefaultFolder);

    public void Update(Func<ScreenshotSettings, ScreenshotSettings> change)
    {
        ArgumentNullException.ThrowIfNull(change);
        store.Save(Section, change(Settings), ScreenshotSettingsJson.Default.ScreenshotSettings);
        Changed?.Invoke();
    }

    /// <summary>Records a folder picked in the Save dialog; <paramref name="makeDefault"/> = "Always save here".</summary>
    public void Remember(string folder, bool makeDefault) =>
        Update(settings =>
        {
            settings = settings with { LastFolder = folder };
            return makeDefault ? WithDefault(settings, folder) : settings;
        });

    /// <summary>Sets the default folder from Settings → Save to.</summary>
    public void SetDefaultFolder(string folder) => Update(settings => WithDefault(settings, folder));

    /// <summary>Back to <c>Pictures\Snapricot</c>.</summary>
    public void ResetDefaultFolder() => Update(settings => settings with { DefaultFolder = null });

    public void DismissSaveTip() => Update(settings => settings with { SaveTipDismissed = true });

    // Choosing Pictures\Snapricot again resets to "no custom default".
    private ScreenshotSettings WithDefault(ScreenshotSettings settings, string folder) =>
        settings with { DefaultFolder = SameFolder(folder, FallbackFolder) ? null : folder };

    private static string? Usable(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) ? folder : null;

    private static bool SameFolder(string a, string b) =>
        string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(a)), Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
