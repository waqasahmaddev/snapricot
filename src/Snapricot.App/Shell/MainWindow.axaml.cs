using Avalonia.Controls;

namespace Snapricot.App.Shell;

public partial class MainWindow : Window
{
    private bool _allowClose;

    public MainWindow()
    {
        InitializeComponent();
    }

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
        // Closing the window keeps Snapricot running in the tray.
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }
}
