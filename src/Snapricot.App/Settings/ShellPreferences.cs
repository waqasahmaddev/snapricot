using System.Text.Json.Serialization;
using Snapricot.Core.Settings;

namespace Snapricot.App.Settings;

/// <summary>Shell settings (section <c>"shell"</c> in settings.json).</summary>
public sealed record ShellSettings
{
    /// <summary>Closing the main window keeps Snapricot running in the tray (otherwise it quits).</summary>
    public bool CloseToTray { get; set; } = true;
}

[JsonSerializable(typeof(ShellSettings))]
internal sealed partial class ShellSettingsJson : JsonSerializerContext;

public sealed class ShellPreferences(ISettingsStore store)
{
    public const string Section = "shell";

    public ShellSettings Settings => store.Load(Section, ShellSettingsJson.Default.ShellSettings, () => new ShellSettings());

    public void Update(Func<ShellSettings, ShellSettings> change) =>
        store.Save(Section, change(Settings), ShellSettingsJson.Default.ShellSettings);
}
