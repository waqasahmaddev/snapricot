using Avalonia.Controls;

namespace Snapricot.App.Shell;

public partial class MainWindow : Window
{
    private bool _allowClose;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>Asked when the user closes the window: true keeps Snapricot in the tray, false quits.</summary>
    public Func<bool> KeepInTrayOnClose { get; set; } = () => true;

    /// <summary>Raised when closing should quit the app (the "keep in tray" setting is off).</summary>
    public event Action? QuitRequested;

    public void ShowAndActivate()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    public void AllowClose() => _allowClose = true;

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            if (KeepInTrayOnClose())
            {
                Hide();
            }
            else
            {
                QuitRequested?.Invoke();
            }
        }

        base.OnClosing(e);
    }
}
