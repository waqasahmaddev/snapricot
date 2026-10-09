using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Capture;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Platform;

namespace Snapricot.Platform.MacOS;

public static class MacOSPlatformServiceCollectionExtensions
{
    /// <summary>
    /// Registers the macOS implementations of the Core.Platform interfaces. Until v2.0 (ADR-0003) these are
    /// placeholders that throw <see cref="PlatformNotSupportedException"/>, so the app still starts and builds on macOS.
    /// v2.0 replaces them with ScreenCaptureKit, Carbon hotkeys and NSPasteboard.
    /// </summary>
    public static IServiceCollection AddMacOSPlatform(this IServiceCollection services)
    {
        services.AddSingleton<UnsupportedPlatform>();
        services.AddSingleton<IScreenCapture>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<IDesktopService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<IHotkeyService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<IAutostartService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<IShellService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        services.AddSingleton<ISoundService>(sp => sp.GetRequiredService<UnsupportedPlatform>());
        return services;
    }

    private sealed class UnsupportedPlatform
        : IScreenCapture, IDesktopService, IHotkeyService, IClipboardService, IAutostartService, IShellService, ISoundService
    {
        private static PlatformNotSupportedException NotYet() => new("macOS support arrives in Snapricot v2.0.");

        public IReadOnlyList<DisplayInfo> GetDisplays() => throw NotYet();

        public Task<CapturedImage> CaptureAsync(PixelBounds bounds, CancellationToken cancellationToken) => throw NotYet();

        public PixelPoint GetCursorPosition() => throw NotYet();

        public PixelBounds? GetForegroundWindowBounds() => throw NotYet();

        public void RefreshCursor()
        {
            // Nothing to do until v2.0.
        }

        public bool TrySetExcludedFromCapture(nint windowHandle, bool excluded) => false;

        public Task<IDisposable> RegisterAsync(HotkeyGesture gesture, Func<CancellationToken, Task> onPressed, CancellationToken cancellationToken) =>
            throw NotYet();

        public Task SetImageAsync(CapturedImage image, CancellationToken cancellationToken) => throw NotYet();

        public bool IsEnabled => false;

        public void SetEnabled(bool enabled) => throw NotYet();

        public void RevealInFolder(string filePath) => throw NotYet();

        public void OpenFile(string filePath) => throw NotYet();

        public void PlayCaptureSound()
        {
            // No sound until v2.0; silence is harmless.
        }
    }
}
