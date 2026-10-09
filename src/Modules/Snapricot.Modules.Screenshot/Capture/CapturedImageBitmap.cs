using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

internal static class CapturedImageBitmap
{
    /// <summary>Copies the pixels into an Avalonia bitmap at 96 DPI, so 1 bitmap unit = 1 physical pixel.</summary>
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
