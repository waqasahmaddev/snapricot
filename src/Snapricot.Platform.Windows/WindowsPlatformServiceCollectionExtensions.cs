using Microsoft.Extensions.DependencyInjection;

namespace Snapricot.Platform.Windows;

public static class WindowsPlatformServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Windows implementations of the Core.Platform interfaces.
    /// Phase 1 adds IScreenCapture, IHotkeyService and IClipboardService here (see docs/plan/roadmap.md).
    /// </summary>
    public static IServiceCollection AddWindowsPlatform(this IServiceCollection services) => services;
}
