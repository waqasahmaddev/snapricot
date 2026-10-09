using Snapricot.Core.Settings;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Tests.Capture;

public sealed class ScreenshotPreferencesTests : IDisposable
{
    private readonly TempFolder _temp = new();
    private readonly string _settingsFile;
    private readonly string _fallback;

    public ScreenshotPreferencesTests()
    {
        _settingsFile = Path.Combine(_temp.Path, "settings.json");
        _fallback = _temp.Sub("Pictures-Snapricot");
    }

    public void Dispose() => _temp.Dispose();

    private ScreenshotPreferences Create() => new(new JsonFileSettingsStore(_settingsFile), _fallback);

    [Fact]
    public void NothingChosen_EverythingIsTheFallback()
    {
        var locations = Create();

        Assert.Equal(_fallback, locations.DefaultFolder);
        Assert.Equal(_fallback, locations.SuggestedFolder);
        Assert.False(locations.DefaultIsMissing);
    }

    [Fact]
    public void Remember_WithoutDefault_OnlyChangesTheSuggestion()
    {
        var other = _temp.Sub("Other");

        Create().Remember(other, makeDefault: false);
        var reloaded = Create();

        Assert.Equal(other, reloaded.SuggestedFolder);
        Assert.Equal(_fallback, reloaded.DefaultFolder);
    }

    [Fact]
    public void Remember_WithDefault_PersistsAcrossRestarts()
    {
        var work = _temp.Sub("Work");

        Create().Remember(work, makeDefault: true);

        Assert.Equal(work, Create().DefaultFolder);
    }

    [Fact]
    public void ChoosingTheFallbackAsDefault_ResetsToNoCustomDefault()
    {
        var locations = Create();
        locations.Remember(_temp.Sub("Work"), makeDefault: true);

        locations.Remember(_fallback, makeDefault: true);

        Assert.Null(locations.Settings.DefaultFolder);
    }

    [Fact]
    public void MissingDefault_IsReported_AndFallsBack()
    {
        var usb = _temp.Sub("Usb");
        var locations = Create();
        locations.Remember(usb, makeDefault: true);
        Directory.Delete(usb);

        Assert.True(locations.DefaultIsMissing);
        Assert.Equal(_fallback, locations.DefaultFolder);
        Assert.Equal(_fallback, locations.SuggestedFolder);
    }

    [Fact]
    public void IsDefault_IgnoresTrailingSeparators()
    {
        Assert.True(Create().IsDefault(_fallback + Path.DirectorySeparatorChar));
    }

    [Fact]
    public void DismissSaveTip_IsRemembered()
    {
        Create().DismissSaveTip();

        Assert.True(Create().Settings.SaveTipDismissed);
    }

    [Fact]
    public void SetDefaultFolder_ThenReset_GoesBackToTheFallback()
    {
        var locations = Create();
        locations.SetDefaultFolder(_temp.Sub("Work"));

        locations.ResetDefaultFolder();

        Assert.Equal(_fallback, Create().DefaultFolder);
    }

    [Fact]
    public void Update_RaisesChanged_AndPersists()
    {
        var locations = Create();
        var raised = 0;
        locations.Changed += () => raised++;

        locations.Update(s => s with { Format = ImageFormat.Jpg, PlaySound = true });

        Assert.Equal(1, raised);
        Assert.Equal((ImageFormat.Jpg, true), (Create().Settings.Format, Create().Settings.PlaySound));
    }

    [Fact]
    public void Defaults_MatchTheApprovedSettings()
    {
        var settings = Create().Settings;

        Assert.Equal(
            (ImageFormat.Png, true, false, true, false, false),
            (settings.Format, settings.AutoSaveOnCopy, settings.OpenFolderAfterSave, settings.ShowNotification, settings.PlaySound, settings.ShowMagnifier));
    }

    [Fact]
    public void OldSettingsFile_MissingNewOptions_KeepsTheirDefaults()
    {
        // Regression: source-generated JSON reset missing init-only properties to false (ADR-0012).
        File.WriteAllText(_settingsFile, """{ "screenshot": { "LastFolder": null, "FileNamePattern": "{date}_{time}" } }""");

        var settings = Create().Settings;

        Assert.True(settings.AutoSaveOnCopy);
        Assert.True(settings.ShowNotification);
        Assert.Equal(ImageFormat.Png, settings.Format);
    }
}
