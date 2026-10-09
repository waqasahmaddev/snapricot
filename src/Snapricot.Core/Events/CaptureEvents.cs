using Snapricot.Core.Capture;

namespace Snapricot.Core.Events;

/// <summary>Published as soon as pixels are captured, before anything is written to disk.</summary>
public sealed record CaptureCompleted(CapturedImage Image, string SourceId);

/// <summary>Published after a capture has been written to disk.</summary>
public sealed record CaptureSaved(CapturedImage Image, string FilePath);
