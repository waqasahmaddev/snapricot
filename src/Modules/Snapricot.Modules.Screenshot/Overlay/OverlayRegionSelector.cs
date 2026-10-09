using Avalonia.Threading;
using Snapricot.Core.Platform;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>
/// Shows a <see cref="RegionOverlayWindow"/> on every display and waits for the first result. Windows are kept and
/// reused while the display layout is unchanged, so later captures open instantly with no window creation.
/// </summary>
public sealed class OverlayRegionSelector(ScreenshotPreferences preferences, IDesktopService desktop) : IRegionSelector
{
    private readonly List<RegionOverlayWindow> _pool = [];

    public async Task<RegionSelection?> SelectAsync(IReadOnlyList<FrozenDisplay> displays, Action? onShown, CancellationToken cancellationToken)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => SelectAsync(displays, onShown, cancellationToken));
        }

        var result = new TaskCompletionSource<RegionSelection?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var windows = TakeWindows(displays);
        var showMagnifier = preferences.Settings.ShowMagnifier;

        // onShown runs once, after the first overlay is actually visible.
        var shown = onShown;
        void OnRevealed()
        {
            var callback = shown;
            shown = null;
            callback?.Invoke();
        }

        await using var registration = cancellationToken.Register(() => result.TrySetResult(null));
        try
        {
            for (var i = 0; i < displays.Count; i++)
            {
                windows[i].Begin(displays[i], showMagnifier, selection => result.TrySetResult(selection), OnRevealed);
            }

            windows.LastOrDefault()?.Activate();
            return await result.Task;
        }
        finally
        {
            foreach (var window in windows)
            {
                window.End();
            }
        }
    }

    /// <summary>One window per display, reusing pooled windows already sized for the same display.</summary>
    private List<RegionOverlayWindow> TakeWindows(IReadOnlyList<FrozenDisplay> displays)
    {
        var available = new List<RegionOverlayWindow>(_pool);
        var chosen = new List<RegionOverlayWindow>(displays.Count);
        foreach (var display in displays)
        {
            var window = available.FirstOrDefault(w => w.Display == display.Info) ?? available.FirstOrDefault(w => w.Display is null);
            if (window is null)
            {
                window = new RegionOverlayWindow(desktop.RefreshCursor);
                _pool.Add(window);
            }

            available.Remove(window);
            chosen.Add(window);
        }

        // Displays that disappeared or changed (unplugged monitor, new resolution): drop their windows.
        foreach (var stale in available)
        {
            _pool.Remove(stale);
            stale.Close();
        }

        return chosen;
    }
}
