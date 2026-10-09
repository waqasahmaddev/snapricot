using Avalonia.Threading;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>Opens a <see cref="RegionOverlayWindow"/> on every display and waits for the first result.</summary>
public sealed class OverlayRegionSelector(ScreenshotPreferences locations) : IRegionSelector
{
    public async Task<RegionSelection?> SelectAsync(IReadOnlyList<FrozenDisplay> displays, CancellationToken cancellationToken)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => SelectAsync(displays, cancellationToken));
        }

        var result = new TaskCompletionSource<RegionSelection?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var showMagnifier = locations.Settings.ShowMagnifier;
        var windows = displays
            .Select(display => new RegionOverlayWindow(display, showMagnifier, selection => result.TrySetResult(selection)))
            .ToList();

        await using var registration = cancellationToken.Register(() => result.TrySetResult(null));
        try
        {
            foreach (var window in windows)
            {
                window.Show();
            }

            windows.LastOrDefault()?.Activate();
            return await result.Task;
        }
        finally
        {
            foreach (var window in windows)
            {
                window.Close();
            }
        }
    }
}
