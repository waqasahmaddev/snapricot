using Snapricot.Core.Modules;

namespace Snapricot.App.Shell;

/// <summary><see cref="IShellWindow"/> for the main window; the window is attached once it exists.</summary>
internal sealed class ShellWindow : IShellWindow
{
    // Long enough for DWM to stop compositing the hidden window before the screen is frozen.
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(180);

    private MainWindow? _window;

    public void Attach(MainWindow window) => _window = window;

    public async Task RunHiddenAsync(Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);
        var window = _window;
        if (window is not { IsVisible: true })
        {
            await work();
            return;
        }

        window.Hide();
        await Task.Delay(SettleDelay);
        try
        {
            await work();
        }
        finally
        {
            window.ShowAndActivate();
        }
    }
}
