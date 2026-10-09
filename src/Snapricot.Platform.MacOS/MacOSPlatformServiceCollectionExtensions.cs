using Microsoft.Extensions.DependencyInjection;

namespace Snapricot.Platform.MacOS;

public static class MacOSPlatformServiceCollectionExtensions
{
    /// <summary>
    /// Registers the macOS implementations of the Core.Platform interfaces.
    /// v2.0 adds ScreenCaptureKit capture, Carbon hotkeys and NSPasteboard here (see docs/plan/roadmap.md).
    /// </summary>
    public static IServiceCollection AddMacOSPlatform(this IServiceCollection services) => services;
}
