using Snapricot.Core.Capture;
using Snapricot.Core.Platform;
using Snapricot.Modules.Screenshot.Capture;
using Snapricot.Modules.Screenshot.Overlay;

namespace Snapricot.Modules.Screenshot.Tests.Capture;

internal static class Images
{
    public static CapturedImage Solid(int width, int height) =>
        new(width, height, width * 4, new byte[width * height * 4], new DateTimeOffset(2026, 10, 9, 13, 11, 20, TimeSpan.Zero));
}

/// <summary>A unique temp directory per test, deleted afterwards.</summary>
internal sealed class TempFolder : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "snapricot-tests", Guid.NewGuid().ToString("N"));

    public TempFolder() => Directory.CreateDirectory(Path);

    public string Sub(string name)
    {
        var path = System.IO.Path.Combine(Path, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}

internal sealed class FakeScreenCapture(params DisplayInfo[] displays) : IScreenCapture
{
    public List<PixelBounds> Captured { get; } = [];

    public IReadOnlyList<DisplayInfo> GetDisplays() => displays;

    public Task<CapturedImage> CaptureAsync(PixelBounds bounds, CancellationToken cancellationToken)
    {
        Captured.Add(bounds);
        return Task.FromResult(Images.Solid(bounds.Width, bounds.Height));
    }
}

internal sealed class FakeDesktop : IDesktopService
{
    public PixelPoint Cursor { get; set; }

    public PixelBounds? ForegroundWindow { get; set; }

    public PixelPoint GetCursorPosition() => Cursor;

    public PixelBounds? GetForegroundWindowBounds() => ForegroundWindow;
}

internal sealed class FakeClipboard : IClipboardService
{
    public List<CapturedImage> Images { get; } = [];

    public Task SetImageAsync(CapturedImage image, CancellationToken cancellationToken)
    {
        Images.Add(image);
        return Task.CompletedTask;
    }
}

internal sealed class FakeRegionSelector : IRegionSelector
{
    public Func<IReadOnlyList<FrozenDisplay>, RegionSelection?> Choose { get; set; } = _ => null;

    public Task<RegionSelection?> SelectAsync(IReadOnlyList<FrozenDisplay> displays, CancellationToken cancellationToken) =>
        Task.FromResult(Choose(displays));
}

internal sealed class FakeSaveDialog : ISaveDialog
{
    public SaveRequest? Answer { get; set; }

    public List<string> SuggestedNames { get; } = [];

    public Task<SaveRequest?> AskAsync(CapturedImage image, string suggestedFileName, CancellationToken cancellationToken)
    {
        SuggestedNames.Add(suggestedFileName);
        return Task.FromResult(Answer);
    }
}

internal sealed class FakeStorage : ICaptureStorage
{
    public List<(CapturedImage Image, string Folder, string FileName, ImageFormat Format)> Saved { get; } = [];

    public Task<string> SaveAsync(CapturedImage image, string folder, string fileName, ImageFormat format, CancellationToken cancellationToken)
    {
        Saved.Add((image, folder, fileName, format));
        return Task.FromResult(System.IO.Path.Combine(folder, fileName + format.Extension()));
    }
}

internal sealed class FakeFeedback : ICaptureFeedback
{
    public int Taken { get; private set; }

    public List<CaptureDelivery> Deliveries { get; } = [];

    public void CaptureTaken() => Taken++;

    public void Delivered(CaptureDelivery delivery) => Deliveries.Add(delivery);
}

internal sealed class FakeShell : IShellService
{
    public List<string> Revealed { get; } = [];

    public void RevealInFolder(string filePath) => Revealed.Add(filePath);

    public void OpenFile(string filePath)
    {
    }
}
