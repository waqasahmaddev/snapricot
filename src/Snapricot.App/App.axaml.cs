using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Snapricot.App.Composition;
using Snapricot.App.Diagnostics;
using Snapricot.App.Settings;
using Snapricot.App.Shell;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Modules;
using Snapricot.Core.Platform;

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

            // Tray app: closing the window hides it (by default); only "Quit" ends the process.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var shellPreferences = _services.GetRequiredService<ShellPreferences>();
            var window = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainWindowViewModel>(),
                KeepInTrayOnClose = () => shellPreferences.Settings.CloseToTray,
            };

            void Quit()
            {
                window.AllowClose();
                desktop.Shutdown();
            }

            window.QuitRequested += Quit;
            _services.GetRequiredService<ShellWindow>().Attach(window);

            // Started by Windows at login ("Start with Windows"): stay in the tray until the user opens the window.
            var startInTray = desktop.Args?.Contains(IAutostartService.StartInTrayArgument) == true;
            if (!startInTray)
            {
                desktop.MainWindow = window;
            }

            TrayMenu.Install(this, _services.GetRequiredService<ModuleCatalog>(), _services, window.ShowAndActivate, Quit);

            // Not awaited: hotkey registration must never delay the first frame. Failures show up in Settings → Hotkeys.
            var hotkeys = _services.GetRequiredService<HotkeyManager>();
            hotkeys.Dispatch = work => Dispatcher.UIThread.InvokeAsync(work);
            _ = hotkeys.RegisterAllAsync(CancellationToken.None);

            StartupMeasurement.Attach(window, desktop);
            desktop.Exit += (_, _) => _services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
