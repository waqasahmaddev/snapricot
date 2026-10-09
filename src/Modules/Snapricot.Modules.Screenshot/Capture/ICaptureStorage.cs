using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

public interface ICaptureStorage
{
    /// <summary>
    /// Encodes the capture into <paramref name="folder"/> as <paramref name="fileName"/> plus the format's extension.
    /// Never overwrites: an existing name gets " (2)", " (3)", ... Returns the full path written.
    /// </summary>
    Task<string> SaveAsync(CapturedImage image, string folder, string fileName, ImageFormat format, CancellationToken cancellationToken);
}
