using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Snapricot.App.Settings;

public partial class HotkeysSettingsView : UserControl
{
    public HotkeysSettingsView()
    {
        InitializeComponent();

        // Tunnel, so the recorder sees keys before buttons react. PrintScreen often arrives only as KeyUp on Windows.
        AddHandler(KeyDownEvent, OnKey, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnKey, RoutingStrategies.Tunnel);
    }

    private async void OnKey(object? sender, KeyEventArgs e)
    {
        if (DataContext is not HotkeysSettingsViewModel { IsRecording: true } viewModel)
        {
            return;
        }

        e.Handled = true;
        if (e.Key == Key.Escape)
        {
            await viewModel.CancelRecordingAsync();
            return;
        }

        var isPrintScreen = e.Key == Key.Snapshot;
        if (e.RoutedEvent == KeyUpEvent && !isPrintScreen)
        {
            return; // everything except PrintScreen is recorded on KeyDown
        }

        if (KeyGestureText.From(e.Key, e.KeyModifiers) is { } gesture)
        {
            await viewModel.CompleteRecordingAsync(gesture);
        }
    }
}
