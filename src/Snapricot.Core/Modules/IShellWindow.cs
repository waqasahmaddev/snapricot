namespace Snapricot.Core.Modules;

/// <summary>
/// Lets a module hide and restore the main window without referencing the shell, e.g. so a capture started from the
/// app doesn't include Snapricot itself. Captures from hotkeys or the tray don't use this.
/// </summary>
public interface IShellWindow
{
    /// <summary>
    /// Hides the main window (if it's open) for the duration of <paramref name="work"/>, then brings it back.
    /// Waits briefly after hiding so the screen no longer shows the window when capture starts.
    /// </summary>
    Task RunHiddenAsync(Func<Task> work);
}
