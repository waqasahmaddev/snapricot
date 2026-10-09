namespace Snapricot.Core.Capture;

/// <summary>A point in physical (not DPI-scaled) virtual-screen coordinates.</summary>
public readonly record struct PixelPoint(int X, int Y);

/// <summary>A pixel rectangle in physical (not DPI-scaled) virtual-screen coordinates.</summary>
public readonly record struct PixelBounds(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;

    public int Bottom => Y + Height;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(PixelPoint point) =>
        point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;

    /// <summary>The overlapping area, or <see langword="null"/> when the rectangles don't overlap.</summary>
    public PixelBounds? Intersect(PixelBounds other)
    {
        var left = Math.Max(X, other.X);
        var top = Math.Max(Y, other.Y);
        var right = Math.Min(Right, other.Right);
        var bottom = Math.Min(Bottom, other.Bottom);
        return right > left && bottom > top ? new PixelBounds(left, top, right - left, bottom - top) : null;
    }
}

/// <summary>A connected display. <see cref="Scale"/> is 1.0 at 96 DPI, 1.5 at 144 DPI, etc.</summary>
public sealed record DisplayInfo(string Id, PixelBounds Bounds, double Scale, bool IsPrimary);

/// <summary>Raw captured pixels in BGRA8888 format, opaque (alpha = 255), rows top to bottom.</summary>
public sealed record CapturedImage(
    int Width,
    int Height,
    int Stride,
    ReadOnlyMemory<byte> Pixels,
    DateTimeOffset CapturedAt);
