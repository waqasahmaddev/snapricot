using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snapricot.Core.Hotkeys;

namespace Snapricot.App.Settings;

public sealed partial class HotkeyRowViewModel(HotkeysSettingsViewModel owner, string id, string title) : ObservableObject
{
    public string Id { get; } = id;

    public string Title { get; } = title;

    [ObservableProperty]
    public partial string Gesture { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Warning { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    public partial bool IsRecording { get; set; }

    public string ButtonText => IsRecording ? "Cancel" : "Change";

    [RelayCommand]
    private Task ToggleRecordingAsync() => IsRecording ? owner.CancelRecordingAsync() : owner.StartRecordingAsync(this);
}

/// <summary>Settings → Hotkeys: shows every module hotkey, records new combinations, and resets to defaults.</summary>
public sealed partial class HotkeysSettingsViewModel : ObservableObject
{
    private readonly HotkeyManager _manager;
    private HotkeyRowViewModel? _recording;

    public HotkeysSettingsViewModel(HotkeyManager manager)
    {
        _manager = manager;
        Rows = [.. manager.States.Select(s => new HotkeyRowViewModel(this, s.Binding.Id, s.Binding.Title))];
        Refresh();
        manager.Changed += () => Dispatcher.UIThread.Post(Refresh);
    }

    public ObservableCollection<HotkeyRowViewModel> Rows { get; }

    public bool IsRecording => _recording is not null;

    public async Task StartRecordingAsync(HotkeyRowViewModel row)
    {
        await CancelRecordingAsync();
        _recording = row;
        row.IsRecording = true;
        row.Gesture = "Press keys…";
        row.Warning = null;
        await _manager.SuspendAsync(CancellationToken.None); // so pressing a current shortcut doesn't start a capture
    }

    public async Task CancelRecordingAsync()
    {
        if (_recording is not { } row)
        {
            return;
        }

        _recording = null;
        row.IsRecording = false;
        await _manager.ResumeAsync(CancellationToken.None);
    }

    /// <summary>Called by the view with the recorded combination.</summary>
    public async Task CompleteRecordingAsync(string gesture)
    {
        if (_recording is not { } row)
        {
            return;
        }

        _recording = null;
        row.IsRecording = false;
        var status = await _manager.RebindAsync(row.Id, gesture, CancellationToken.None);
        await _manager.ResumeAsync(CancellationToken.None);
        Refresh();
        if (status != HotkeyStatus.Active)
        {
            row.Warning = status == HotkeyStatus.Unavailable
                ? $"{gesture} is in use by another app. Your previous shortcut was kept."
                : $"{gesture} can't be used as a shortcut.";
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        await CancelRecordingAsync();
        await _manager.ResetAllAsync(CancellationToken.None);
    }

    private void Refresh()
    {
        foreach (var state in _manager.States)
        {
            if (Rows.FirstOrDefault(r => r.Id == state.Binding.Id) is not { IsRecording: false } row)
            {
                continue;
            }

            row.Gesture = state.Gesture;
            row.Warning = state.Status switch
            {
                HotkeyStatus.Unavailable => "In use by another app. Pick a different shortcut.",
                HotkeyStatus.Invalid => "This shortcut isn't valid. Pick a different one.",
                _ => row.Warning is { } kept && kept.Contains("previous shortcut was kept", StringComparison.Ordinal) ? kept : null,
            };
        }
    }
}
