namespace Snapricot.Core.Capture;

/// <summary>
/// Extension point: a user-facing way to capture (region, window, full screen, scrolling, delayed, ...).
/// Implemented by modules on top of the platform primitive <see cref="Platform.IScreenCapture"/>.
/// </summary>
public interface ICaptureSource
{
    string Id { get; }

    string Title { get; }

    /// <summary>Returns <see langword="null"/> when the user cancels (e.g. presses Esc).</summary>
    Task<CapturedImage?> CaptureAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Extension point: something the user can do with a capture (copy, save, copy text, upload, ...).
/// Each registered action appears in the capture overlay toolbar.
/// </summary>
public interface IAfterCaptureAction
{
    string Id { get; }

    string Title { get; }

    int Order { get; }

    Task ExecuteAsync(CapturedImage image, CancellationToken cancellationToken);
}
