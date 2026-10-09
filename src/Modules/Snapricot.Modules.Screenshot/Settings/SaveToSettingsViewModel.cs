using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Settings;

/// <summary>Settings → Save to: default folder, file name pattern, format, and what happens after saving.</summary>
public sealed partial class SaveToSettingsViewModel : ObservableObject
{
    private static readonly DateTimeOffset ExampleTime = new(2026, 10, 9, 13, 34, 5, 45, TimeSpan.Zero);
    private readonly ScreenshotPreferences _preferences;

    public SaveToSettingsViewModel(ScreenshotPreferences preferences)
    {
        _preferences = preferences;
        var settings = preferences.Settings;
        Pattern = settings.FileNamePattern;
        FormatIndex = settings.Format == ImageFormat.Jpg ? 1 : 0;
        AutoSaveOnCopy = settings.AutoSaveOnCopy;
        OpenFolderAfterSave = settings.OpenFolderAfterSave;
        OnPatternChanged(Pattern); // fills the example (the generated change hook skips unchanged values)
        RefreshFolder();
        preferences.Changed += RefreshFolder;
    }

    public IReadOnlyList<string> Formats { get; } = ["PNG", "JPG"];

    [ObservableProperty]
    public partial string Folder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDefaultFolder { get; set; }

    [ObservableProperty]
    public partial string? FolderNote { get; set; }

    [ObservableProperty]
    public partial string Pattern { get; set; } = FileNamePattern.Default;

    [ObservableProperty]
    public partial string Example { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? PatternError { get; set; }

    [ObservableProperty]
    public partial int FormatIndex { get; set; }

    [ObservableProperty]
    public partial bool AutoSaveOnCopy { get; set; }

    [ObservableProperty]
    public partial bool OpenFolderAfterSave { get; set; }

    private ImageFormat Format => FormatIndex == 1 ? ImageFormat.Jpg : ImageFormat.Png;

    /// <summary>Called by the view after the folder picker returns a local path.</summary>
    public void ChooseFolder(string folder) => _preferences.SetDefaultFolder(folder);

    [RelayCommand]
    private void ResetFolder() => _preferences.ResetDefaultFolder();

    partial void OnPatternChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            (PatternError, Example) = ("Enter a pattern, for example {date}_{time}.", string.Empty);
            return;
        }

        var name = FileNamePattern.Format(value, ExampleTime);
        PatternError = FileNameRules.Validate(name);
        Example = PatternError is null ? name + Format.Extension() : string.Empty;
        if (PatternError is null && value != _preferences.Settings.FileNamePattern)
        {
            _preferences.Update(s => s with { FileNamePattern = value.Trim() });
        }
    }

    partial void OnFormatIndexChanged(int value)
    {
        _preferences.Update(s => s with { Format = Format });
        OnPatternChanged(Pattern); // refresh the example's extension
    }

    partial void OnAutoSaveOnCopyChanged(bool value) => _preferences.Update(s => s with { AutoSaveOnCopy = value });

    partial void OnOpenFolderAfterSaveChanged(bool value) => _preferences.Update(s => s with { OpenFolderAfterSave = value });

    private void RefreshFolder()
    {
        Folder = _preferences.DefaultFolder;
        IsDefaultFolder = _preferences.Settings.DefaultFolder is null || _preferences.DefaultIsMissing;
        FolderNote = _preferences.DefaultIsMissing
            ? $"Your chosen folder ({_preferences.Settings.DefaultFolder}) isn't available, so captures go here for now."
            : null;
    }
}
