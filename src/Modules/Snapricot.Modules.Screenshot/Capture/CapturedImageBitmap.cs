using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

internal static class CapturedImageBitmap
{
    /// <summary>
    /// An immutable copy at 96 DPI (1 bitmap unit = 1 physical pixel). Immutable bitmaps are uploaded to the GPU once
    /// and cached, which is what makes the overlay smooth; use this for anything that's only displayed.
    /// </summary>
    public static unsafe Bitmap ToDisplayBitmap(CapturedImage image)
    {
        using var pinned = image.Pixels.Pin();
        return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, (nint)pinned.Pointer,
            new PixelSize(image.Width, image.Height), new Vector(96, 96), image.Stride);
    }

    /// <summary>Copies the pixels into a writable Avalonia bitmap at 96 DPI (used for encoding to PNG/JPG).</summary>
    public static unsafe WriteableBitmap ToBitmap(CapturedImage image)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(image.Width, image.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);

        using var buffer = bitmap.Lock();
        var source = image.Pixels.Span;
        var rowBytes = image.Width * 4;
        for (var row = 0; row < image.Height; row++)
        {
            source.Slice(row * image.Stride, rowBytes)
                .CopyTo(new Span<byte>((byte*)buffer.Address + (row * buffer.RowBytes), rowBytes));
        }

        return bitmap;
    }
}
