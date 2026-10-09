using CommunityToolkit.Mvvm.ComponentModel;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Settings;

/// <summary>Screenshot options shown on Settings → General.</summary>
public sealed partial class CaptureSettingsViewModel : ObservableObject
{
    private readonly ScreenshotPreferences _preferences;

    public CaptureSettingsViewModel(ScreenshotPreferences preferences)
    {
        _preferences = preferences;
        var settings = preferences.Settings;
        ShowMagnifier = settings.ShowMagnifier;
        ShowNotification = settings.ShowNotification;
        PlaySound = settings.PlaySound;
    }

    [ObservableProperty]
    public partial bool ShowMagnifier { get; set; }

    [ObservableProperty]
    public partial bool ShowNotification { get; set; }

    [ObservableProperty]
    public partial bool PlaySound { get; set; }

    partial void OnShowMagnifierChanged(bool value) => _preferences.Update(s => s with { ShowMagnifier = value });

    partial void OnShowNotificationChanged(bool value) => _preferences.Update(s => s with { ShowNotification = value });

    partial void OnPlaySoundChanged(bool value) => _preferences.Update(s => s with { PlaySound = value });
}
