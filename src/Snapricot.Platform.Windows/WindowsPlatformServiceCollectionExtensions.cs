using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Platform;

namespace Snapricot.Platform.Windows;

public static class WindowsPlatformServiceCollectionExtensions
{
    /// <summary>Registers the Windows implementations of the Core.Platform interfaces.</summary>
    public static IServiceCollection AddWindowsPlatform(this IServiceCollection services)
    {
        services.AddSingleton<Win32MessageThread>();
        services.AddSingleton<IScreenCapture, GdiScreenCapture>();
        services.AddSingleton<IDesktopService, Win32DesktopService>();
        services.AddSingleton<IHotkeyService, Win32HotkeyService>();
        services.AddSingleton<IClipboardService, Win32ClipboardService>();
        services.AddSingleton<IAutostartService, Win32AutostartService>();
        services.AddSingleton<IShellService, Win32ShellService>();
        services.AddSingleton<ISoundService, Win32SoundService>();
        return services;
    }
}
