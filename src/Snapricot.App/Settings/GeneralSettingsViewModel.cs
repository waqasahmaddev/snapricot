using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Snapricot.Core.Platform;

namespace Snapricot.App.Settings;

/// <summary>The shell's options on Settings → General.</summary>
public sealed partial class GeneralSettingsViewModel : ObservableObject
{
    private readonly IAutostartService _autostart;
    private readonly ShellPreferences _preferences;
    private readonly ILogger<GeneralSettingsViewModel> _logger;
    private bool _loading = true;

    public GeneralSettingsViewModel(IAutostartService autostart, ShellPreferences preferences, ILogger<GeneralSettingsViewModel> logger)
    {
        _autostart = autostart;
        _preferences = preferences;
        _logger = logger;
        StartWithWindows = autostart.IsEnabled;
        CloseToTray = preferences.Settings.CloseToTray;
        _loading = false;
    }

    [ObservableProperty]
    public partial bool StartWithWindows { get; set; }

    [ObservableProperty]
    public partial bool CloseToTray { get; set; }

    [ObservableProperty]
    public partial string? Error { get; set; }

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_loading)
        {
            return;
        }

        try
        {
            _autostart.SetEnabled(value);
            Error = null;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException or System.Security.SecurityException)
        {
            LogAutostartFailed(ex);
            Error = "Couldn't change the startup setting. Try again, or check whether a policy blocks startup apps.";
        }
    }

    partial void OnCloseToTrayChanged(bool value)
    {
        if (!_loading)
        {
            _preferences.Update(s => s with { CloseToTray = value });
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Changing start-with-Windows failed")]
    private partial void LogAutostartFailed(Exception exception);
}
