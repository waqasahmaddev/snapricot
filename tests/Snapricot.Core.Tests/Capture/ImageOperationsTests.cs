using Snapricot.Core.Capture;

namespace Snapricot.Core.Tests.Capture;

public sealed class ImageOperationsTests
{
    private static readonly DateTimeOffset CapturedAt = new(2026, 10, 9, 14, 32, 7, TimeSpan.Zero);

    /// <summary>A 4x3 image where pixel (x, y) has blue = x, green = y.</summary>
    private static CapturedImage Gradient(int width = 4, int height = 3, int padding = 0)
    {
        var stride = (width * 4) + padding;
        var pixels = new byte[stride * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = (y * stride) + (x * 4);
                pixels[i] = (byte)x;
                pixels[i + 1] = (byte)y;
                pixels[i + 3] = 255;
            }
        }

        return new CapturedImage(width, height, stride, pixels, CapturedAt);
    }

    private static (int X, int Y) PixelAt(CapturedImage image, int x, int y)
    {
        var i = (y * image.Stride) + (x * 4);
        return (image.Pixels.Span[i], image.Pixels.Span[i + 1]);
    }

    [Fact]
    public void Crop_CopiesTheRequestedArea()
    {
        var cropped = ImageOperations.Crop(Gradient(), new PixelBounds(1, 1, 2, 2));

        Assert.Equal((2, 2, 8), (cropped.Width, cropped.Height, cropped.Stride));
        Assert.Equal((1, 1), PixelAt(cropped, 0, 0));
        Assert.Equal((2, 2), PixelAt(cropped, 1, 1));
    }

    [Fact]
    public void Crop_HonoursSourceStridePadding()
    {
        var cropped = ImageOperations.Crop(Gradient(padding: 12), new PixelBounds(3, 2, 1, 1));

        Assert.Equal((3, 2), PixelAt(cropped, 0, 0));
    }

    [Fact]
    public void Crop_KeepsCaptureTimestamp()
    {
        Assert.Equal(CapturedAt, ImageOperations.Crop(Gradient(), new PixelBounds(0, 0, 1, 1)).CapturedAt);
    }

    [Fact]
    public void Crop_FullImage_ReturnsEqualPixels()
    {
        var image = Gradient();

        var cropped = ImageOperations.Crop(image, new PixelBounds(0, 0, 4, 3));

        Assert.Equal(image.Pixels.ToArray(), cropped.Pixels.ToArray());
    }

    [Theory]
    [InlineData(-1, 0, 2, 2)]
    [InlineData(0, 0, 5, 1)]
    [InlineData(3, 2, 2, 1)]
    [InlineData(0, 0, 0, 1)]
    public void Crop_AreaOutsideImage_Throws(int x, int y, int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageOperations.Crop(Gradient(), new PixelBounds(x, y, width, height)));
    }
}
