using Snapricot.Core.Modules;
using Snapricot.Core.Platform;

namespace Snapricot.App.Shell;

/// <summary><see cref="IShellWindow"/> for the main window; the window is attached once it exists.</summary>
internal sealed class ShellWindow(IDesktopService desktop) : IShellWindow
{
    // Lets DWM apply "excluded from capture" before the screen is frozen (verified with GDI BitBlt on Windows 11).
    private static readonly TimeSpan ExcludeSettleDelay = TimeSpan.FromMilliseconds(80);

    // Where exclusion isn't supported (older Windows), hide instead and wait out Windows 11's fade-out.
    private static readonly TimeSpan FadeDelay = TimeSpan.FromMilliseconds(250);

    private MainWindow? _window;

    public void Attach(MainWindow window) => _window = window;

    public async Task<IShellCaptureScope> BeginCaptureAsync()
    {
        if (_window is not { IsVisible: true } window)
        {
            return Scope.None;
        }

        var handle = window.TryGetPlatformHandle()?.Handle ?? 0;
        if (handle != 0 && desktop.TrySetExcludedFromCapture(handle, excluded: true))
        {
            await Task.Delay(ExcludeSettleDelay);
            return new Scope(window, handle, desktop);
        }

        var fallback = new Scope(window, 0, desktop);
        fallback.HideWindow();
        await Task.Delay(FadeDelay);
        return fallback;
    }

    private sealed class Scope(MainWindow? window, nint excludedHandle, IDesktopService? desktop) : IShellCaptureScope
    {
        public static readonly Scope None = new(null, 0, null);

        private bool _hidden;

        public void HideWindow()
        {
            if (window is { IsVisible: true })
            {
                window.Hide();
                _hidden = true;
            }
        }

        public ValueTask DisposeAsync()
        {
            if (excludedHandle != 0)
            {
                desktop?.TrySetExcludedFromCapture(excludedHandle, excluded: false);
            }

            if (_hidden)
            {
                window?.ShowAndActivate();
            }

            return ValueTask.CompletedTask;
        }
    }
}
