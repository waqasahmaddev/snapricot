using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapricot.App.Shell;
using Snapricot.Core;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Modules;
using Snapricot.Core.Settings;
#if SNAPRICOT_WINDOWS
using Snapricot.Platform.Windows;
#else
using Snapricot.Platform.MacOS;
#endif

namespace Snapricot.App.Composition;

/// <summary>The composition root: the only place that wires Core, the platform, and modules together.</summary>
internal static class AppHost
{
    public static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(logging => logging.AddDebug().SetMinimumLevel(LogLevel.Information));
        services.AddSnapricotCore();
        services.AddSingleton<ISettingsStore>(new JsonFileSettingsStore(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Snapricot", "settings.json")));
#if SNAPRICOT_WINDOWS
        services.AddWindowsPlatform();
#else
        services.AddMacOSPlatform();
#endif
        ModuleLoader.Load(services, InstalledModules.All);
        services.AddSingleton<HotkeyManager>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
