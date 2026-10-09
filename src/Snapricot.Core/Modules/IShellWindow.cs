namespace Snapricot.Core.Modules;

/// <summary>
/// Lets a module keep the main window out of a capture started from the app, without referencing the shell.
/// Captures from hotkeys or the tray don't use this.
/// </summary>
public interface IShellWindow
{
    /// <summary>
    /// Excludes the main window from screen capture (it stays on screen, so the mouse stays over Snapricot) and waits
    /// until that applies. Dispose the result when the capture is finished to restore (and re-show) the window.
    /// </summary>
    Task<IShellCaptureScope> BeginCaptureAsync();
}

/// <summary>An in-progress capture started from the app.</summary>
public interface IShellCaptureScope : IAsyncDisposable
{
    /// <summary>Hides the main window (e.g. once the overlay covers it); it's shown again on dispose.</summary>
    void HideWindow();
}
