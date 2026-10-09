namespace Snapricot.Core.Capture;

/// <summary>A pixel rectangle in physical (not DPI-scaled) screen coordinates.</summary>
public readonly record struct PixelBounds(int X, int Y, int Width, int Height);

/// <summary>A connected display. <see cref="Scale"/> is 1.0 at 96 DPI, 1.5 at 144 DPI, etc.</summary>
public sealed record DisplayInfo(string Id, PixelBounds Bounds, double Scale, bool IsPrimary);

/// <summary>Raw captured pixels in BGRA8888 premultiplied format.</summary>
public sealed record CapturedImage(
    int Width,
    int Height,
    int Stride,
    ReadOnlyMemory<byte> Pixels,
    DateTimeOffset CapturedAt);
