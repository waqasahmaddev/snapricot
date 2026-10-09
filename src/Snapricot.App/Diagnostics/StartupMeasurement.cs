using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Snapricot.App.Diagnostics;

/// <summary>
/// <c>Snapricot --measure-startup result.json</c> writes the time from process start until the main window
/// is open, plus memory, then exits. CI uses it to track the performance budgets (docs/architecture/performance.md).
/// </summary>
internal static class StartupMeasurement
{
    private static string? _outputPath;

    public static void Configure(string[] args)
    {
        var index = Array.IndexOf(args, "--measure-startup");
        if (index >= 0 && index + 1 < args.Length)
        {
            _outputPath = args[index + 1];
        }
    }

    public static void Attach(Window window, IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_outputPath is null)
        {
            return;
        }

        window.Opened += (_, _) =>
        {
            using var process = Process.GetCurrentProcess();
            var startupMs = (DateTime.Now - process.StartTime).TotalMilliseconds;
            var workingSetMb = process.WorkingSet64 / 1024d / 1024d;

            File.WriteAllText(_outputPath, string.Create(
                CultureInfo.InvariantCulture,
                $$"""{"startupMs": {{startupMs:F0}}, "workingSetMb": {{workingSetMb:F1}}}"""));

            if (window is Shell.MainWindow main)
            {
                main.AllowClose();
            }

            // Opened fires inside Show(), before the lifetime has started its main loop; shutting down
            // synchronously here would tear the lifetime down mid-start.
            Dispatcher.UIThread.Post(() => desktop.Shutdown(), DispatcherPriority.Background);
        };
    }
}
