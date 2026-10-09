namespace Snapricot.Core.Capture;

public static class ImageOperations
{
    private const int BytesPerPixel = 4;

    /// <summary>Copies <paramref name="area"/> (in image pixel coordinates) out of <paramref name="image"/>.</summary>
    public static CapturedImage Crop(CapturedImage image, PixelBounds area)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (area.IsEmpty || area.X < 0 || area.Y < 0 || area.Right > image.Width || area.Bottom > image.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(area), area, $"Area must lie inside the {image.Width}x{image.Height} image.");
        }

        var stride = area.Width * BytesPerPixel;
        var pixels = new byte[stride * area.Height];
        var source = image.Pixels.Span;

        for (var row = 0; row < area.Height; row++)
        {
            source.Slice(((area.Y + row) * image.Stride) + (area.X * BytesPerPixel), stride)
                .CopyTo(pixels.AsSpan(row * stride, stride));
        }

        return new CapturedImage(area.Width, area.Height, stride, pixels, image.CapturedAt);
    }
}
