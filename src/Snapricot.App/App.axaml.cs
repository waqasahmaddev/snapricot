using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Snapricot.App.Composition;
using Snapricot.App.Diagnostics;
using Snapricot.App.Shell;
using Snapricot.Core.Modules;

namespace Snapricot.App;

public partial class App : Application
{
    private ServiceProvider? _services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _services = AppHost.BuildServices();

            // Tray app: closing the window hides it; only "Quit" ends the process.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var window = new MainWindow { DataContext = _services.GetRequiredService<MainWindowViewModel>() };
            desktop.MainWindow = window;

            TrayMenu.Install(this, _services.GetRequiredService<ModuleCatalog>(), _services, window.ShowAndActivate, () =>
            {
                window.AllowClose();
                desktop.Shutdown();
            });

            StartupMeasurement.Attach(window, desktop);
            desktop.Exit += (_, _) => _services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
