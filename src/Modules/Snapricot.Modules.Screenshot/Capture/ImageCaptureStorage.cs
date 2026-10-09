using Avalonia.Media.Imaging;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>Saves captures as PNG or JPG using Avalonia's Skia encoders, off the UI thread.</summary>
public sealed class ImageCaptureStorage : ICaptureStorage
{
    public Task<string> SaveAsync(CapturedImage image, string folder, string fileName, ImageFormat format, CancellationToken cancellationToken) =>
        Task.Run(() =>
        {
            Directory.CreateDirectory(folder);
            var path = UniquePath(folder, fileName, format.Extension());

            using var bitmap = CapturedImageBitmap.ToBitmap(image);
            using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            bitmap.Save(file, format == ImageFormat.Jpg ? JpegBitmapEncoderOptions.Default : PngBitmapEncoderOptions.Default);
            return path;
        }, cancellationToken);

    private static string UniquePath(string folder, string name, string extension)
    {
        var path = Path.Combine(folder, name + extension);
        for (var n = 2; File.Exists(path); n++)
        {
            path = Path.Combine(folder, $"{name} ({n}){extension}");
        }

        return path;
    }
}
