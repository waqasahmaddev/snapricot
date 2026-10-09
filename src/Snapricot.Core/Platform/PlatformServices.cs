using Snapricot.Core.Capture;

namespace Snapricot.Core.Platform;

// OS services. Implemented once per OS in Snapricot.Platform.{Windows,MacOS} (ADR-0007).
// Modules depend on these interfaces only and never on a platform project.

/// <summary>Low-level screen pixel access.</summary>
public interface IScreenCapture
{
    IReadOnlyList<DisplayInfo> GetDisplays();

    /// <summary>Capture a rectangle in physical pixels; may span several displays.</summary>
    Task<CapturedImage> CaptureAsync(PixelBounds bounds, CancellationToken cancellationToken);
}

/// <summary>System-wide keyboard shortcuts that work while another app has focus.</summary>
public interface IHotkeyService
{
    /// <summary>Registers <paramref name="gesture"/> (e.g. <c>Ctrl+Shift+S</c>). Dispose to unregister.</summary>
    IDisposable Register(string gesture, Func<CancellationToken, Task> onPressed);
}

public interface IClipboardService
{
    Task SetImageAsync(CapturedImage image, CancellationToken cancellationToken);
}
