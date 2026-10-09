using Snapricot.Core.Capture;
using Snapricot.Core.Hotkeys;

namespace Snapricot.Core.Platform;

// OS services. Implemented once per OS in Snapricot.Platform.{Windows,MacOS} (ADR-0007).
// Modules depend on these interfaces only and never on a platform project.
// All coordinates are physical pixels in virtual-screen space.

/// <summary>Low-level screen pixel access.</summary>
public interface IScreenCapture
{
    IReadOnlyList<DisplayInfo> GetDisplays();

    /// <summary>Capture a rectangle; may span several displays. Runs off the calling thread.</summary>
    Task<CapturedImage> CaptureAsync(PixelBounds bounds, CancellationToken cancellationToken);
}

/// <summary>Queries about the desktop state.</summary>
public interface IDesktopService
{
    PixelPoint GetCursorPosition();

    /// <summary>The visible frame of the focused window, or <see langword="null"/> if there is none.</summary>
    PixelBounds? GetForegroundWindowBounds();

    /// <summary>
    /// Makes the OS re-evaluate the cursor shape now. Windows only updates it when the mouse moves, so without this a
    /// newly shown overlay keeps the cursor of the app underneath (e.g. an editor's text I-beam).
    /// </summary>
    void RefreshCursor();

    /// <summary>
    /// Excludes a native window from screen capture while keeping it on screen (Windows 10 2004+).
    /// Returns <see langword="false"/> when the OS doesn't support it.
    /// </summary>
    bool TrySetExcludedFromCapture(nint windowHandle, bool excluded);
}

/// <summary>System-wide keyboard shortcuts that work while another app has focus.</summary>
public interface IHotkeyService
{
    /// <summary>
    /// Registers <paramref name="gesture"/>; dispose the result to unregister. <paramref name="onPressed"/> runs on a
    /// background thread. Throws <see cref="HotkeyUnavailableException"/> if another app owns the hotkey.
    /// </summary>
    Task<IDisposable> RegisterAsync(
        HotkeyGesture gesture,
        Func<CancellationToken, Task> onPressed,
        CancellationToken cancellationToken);
}

public interface IClipboardService
{
    Task SetImageAsync(CapturedImage image, CancellationToken cancellationToken);
}

/// <summary>"Start with Windows" (login items on macOS). Started this way, the app opens hidden in the tray.</summary>
public interface IAutostartService
{
    /// <summary>Command-line flag passed when the OS starts the app at login.</summary>
    public const string StartInTrayArgument = "--tray";

    bool IsEnabled { get; }

    void SetEnabled(bool enabled);
}

/// <summary>Opening files and folders in the OS file manager.</summary>
public interface IShellService
{
    /// <summary>Opens the folder and selects the file.</summary>
    void RevealInFolder(string filePath);

    /// <summary>Opens the file with its default app.</summary>
    void OpenFile(string filePath);
}

public interface ISoundService
{
    /// <summary>A short camera-shutter sound; never blocks.</summary>
    void PlayCaptureSound();
}
