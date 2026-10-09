using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>What happened to a capture, for the notification.</summary>
/// <param name="SavedPath">The file written, or <see langword="null"/> if it wasn't saved.</param>
public sealed record CaptureDelivery(CapturedImage Image, bool Copied, string? SavedPath);

/// <summary>Sound and notification feedback. An interface so the capture flow is testable without UI or audio.</summary>
public interface ICaptureFeedback
{
    /// <summary>Called the moment pixels exist (plays the shutter sound if enabled).</summary>
    void CaptureTaken();

    /// <summary>Called once the capture is copied and/or saved (shows the notification if enabled).</summary>
    void Delivered(CaptureDelivery delivery);
}
