using Avalonia;
using Snapricot.App.Diagnostics;

namespace Snapricot.App;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        StartupMeasurement.Configure(args);
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Also used by the Avalonia previewer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
