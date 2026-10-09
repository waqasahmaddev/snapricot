using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snapricot.Core.Events;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Views;

public sealed record CaptureModeItem(string Title, string Description, string Gesture, ICommand Capture);

public sealed partial class ScreenshotPageViewModel : ObservableObject, IDisposable
{
    private readonly IDisposable _savedSubscription;

    public ScreenshotPageViewModel(CaptureCoordinator coordinator, IEventBus events)
    {
        Modes =
        [
            new("Region", "Drag to select any part of the screen", ScreenshotModule.RegionGesture,
                new AsyncRelayCommand(() => coordinator.CaptureRegionAsync(CancellationToken.None))),
            new("Window", "Capture the window you're working in", ScreenshotModule.WindowGesture,
                new AsyncRelayCommand(() => coordinator.CaptureActiveWindowAsync(CancellationToken.None))),
            new("Full screen", "Capture the display under the cursor", ScreenshotModule.FullScreenGesture,
                new AsyncRelayCommand(() => coordinator.CaptureFullScreenAsync(CancellationToken.None))),
        ];

        _savedSubscription = events.Subscribe<CaptureSaved>((saved, _) =>
        {
            Dispatcher.UIThread.Post(() =>
                Status = $"Last capture: {saved.Image.Width} × {saved.Image.Height}, copied and saved to {saved.FilePath}");
            return ValueTask.CompletedTask;
        });
    }

    public IReadOnlyList<CaptureModeItem> Modes { get; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Press a shortcut from anywhere, or click a mode below. Esc cancels.";

    public void Dispose() => _savedSubscription.Dispose();
}
