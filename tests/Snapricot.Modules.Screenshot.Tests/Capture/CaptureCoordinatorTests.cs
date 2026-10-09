using Microsoft.Extensions.Logging.Abstractions;
using Snapricot.Core.Capture;
using Snapricot.Core.Events;
using Snapricot.Core.Settings;
using Snapricot.Modules.Screenshot.Capture;
using Snapricot.Modules.Screenshot.Overlay;

namespace Snapricot.Modules.Screenshot.Tests.Capture;

public sealed class CaptureCoordinatorTests : IDisposable
{
    private static readonly DisplayInfo Left = new("left", new PixelBounds(-1920, 0, 1920, 1080), 1.0, IsPrimary: false);
    private static readonly DisplayInfo Primary = new("primary", new PixelBounds(0, 0, 2560, 1440), 1.5, IsPrimary: true);

    private readonly TempFolder _temp = new();
    private readonly FakeScreenCapture _screen = new(Left, Primary);
    private readonly FakeDesktop _desktop = new();
    private readonly FakeClipboard _clipboard = new();
    private readonly FakeRegionSelector _selector = new();
    private readonly FakeSaveDialog _dialog = new();
    private readonly FakeStorage _storage = new();
    private readonly EventBus _events = new(NullLogger<EventBus>.Instance);
    private readonly FakeFeedback _feedback = new();
    private readonly FakeShell _shell = new();
    private readonly List<object> _published = [];
    private readonly ScreenshotPreferences _locations;

    public CaptureCoordinatorTests()
    {
        _locations = new ScreenshotPreferences(new JsonFileSettingsStore(Path.Combine(_temp.Path, "settings.json")), _temp.Sub("Pictures-Snapricot"));
        _events.Subscribe<CaptureCompleted>((e, _) => { _published.Add(e); return ValueTask.CompletedTask; });
        _events.Subscribe<CaptureSaved>((e, _) => { _published.Add(e); return ValueTask.CompletedTask; });
    }

    public void Dispose() => _temp.Dispose();

    private CaptureCoordinator Create(IRegionSelector? selector = null) =>
        new(_screen, _desktop, _clipboard, selector ?? _selector, _dialog, _storage, _locations, _feedback, _shell, _events,
            NullLogger<CaptureCoordinator>.Instance);

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void SelectOnPrimary(CaptureIntent intent) =>
        _selector.Choose = frozen => new RegionSelection(frozen[1], new PixelBounds(10, 20, 300, 200), intent);

    [Fact]
    public async Task FullScreen_CapturesTheDisplayUnderTheCursor()
    {
        _desktop.Cursor = new PixelPoint(-500, 300);

        await Create().CaptureFullScreenAsync(Token);

        Assert.Equal(Left.Bounds, Assert.Single(_screen.Captured));
    }

    [Fact]
    public async Task FullScreen_CursorOffAllDisplays_FallsBackToPrimary()
    {
        _desktop.Cursor = new PixelPoint(99_999, 99_999);

        await Create().CaptureFullScreenAsync(Token);

        Assert.Equal(Primary.Bounds, Assert.Single(_screen.Captured));
    }

    [Fact]
    public async Task Copy_CopiesThenAutoSavesToTheDefaultFolder_AndPublishesInOrder()
    {
        SelectOnPrimary(CaptureIntent.Copy);

        await Create().CaptureRegionAsync(Token);

        var image = Assert.Single(_clipboard.Images);
        Assert.Equal((300, 200), (image.Width, image.Height));
        var saved = Assert.Single(_storage.Saved);
        Assert.Equal((_locations.FallbackFolder, "2026-10-09_13-11-20"), (saved.Folder, saved.FileName));
        Assert.Collection(
            _published,
            e => Assert.Equal(CaptureCoordinator.RegionSource, Assert.IsType<CaptureCompleted>(e).SourceId),
            e => Assert.StartsWith(_locations.FallbackFolder, Assert.IsType<CaptureSaved>(e).FilePath, StringComparison.Ordinal));
        Assert.Empty(_dialog.SuggestedNames);
    }

    [Fact]
    public async Task Copy_UsesTheUsersDefaultFolder_WhenOneIsSet()
    {
        var custom = _temp.Sub("Work");
        _locations.Remember(custom, makeDefault: true);
        SelectOnPrimary(CaptureIntent.Copy);

        await Create().CaptureRegionAsync(Token);

        Assert.Equal(custom, Assert.Single(_storage.Saved).Folder);
    }

    [Fact]
    public async Task Copy_DefaultFolderMissing_FallsBackToPicturesSnapricot()
    {
        var removable = _temp.Sub("UsbDrive");
        _locations.Remember(removable, makeDefault: true);
        Directory.Delete(removable);
        SelectOnPrimary(CaptureIntent.Copy);

        await Create().CaptureRegionAsync(Token);

        Assert.Equal(_locations.FallbackFolder, Assert.Single(_storage.Saved).Folder);
    }

    [Fact]
    public async Task Copy_WithAutoSaveOff_OnlyCopies()
    {
        _locations.Update(s => s with { AutoSaveOnCopy = false });

        await Create().CaptureFullScreenAsync(Token);

        Assert.Single(_clipboard.Images);
        Assert.Empty(_storage.Saved);
        Assert.IsType<CaptureCompleted>(Assert.Single(_published));
        Assert.Equal(new CaptureDelivery(_clipboard.Images[0], Copied: true, SavedPath: null), Assert.Single(_feedback.Deliveries));
    }

    [Fact]
    public async Task Copy_UsesTheChosenFormat()
    {
        _locations.Update(s => s with { Format = ImageFormat.Jpg });

        await Create().CaptureFullScreenAsync(Token);

        Assert.Equal(ImageFormat.Jpg, Assert.Single(_storage.Saved).Format);
    }

    [Fact]
    public async Task Capture_GivesFeedback_TakenThenDelivered()
    {
        await Create().CaptureFullScreenAsync(Token);

        Assert.Equal(1, _feedback.Taken);
        var delivery = Assert.Single(_feedback.Deliveries);
        Assert.True(delivery.Copied);
        Assert.EndsWith(".png", delivery.SavedPath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenFolderAfterSave_RevealsTheSavedFile()
    {
        _locations.Update(s => s with { OpenFolderAfterSave = true });

        await Create().CaptureFullScreenAsync(Token);

        Assert.Equal(_storage.Saved[0].Folder, Path.GetDirectoryName(Assert.Single(_shell.Revealed)));
    }

    [Fact]
    public async Task OpenFolderAfterSave_Off_RevealsNothing()
    {
        await Create().CaptureFullScreenAsync(Token);

        Assert.Empty(_shell.Revealed);
    }

    [Fact]
    public async Task Save_AsksForNameAndFolder_SavesThere_AndDoesNotTouchTheClipboard()
    {
        var chosen = _temp.Sub("Bug reports");
        _dialog.Answer = new SaveRequest(chosen, "login-error", MakeDefault: false);
        SelectOnPrimary(CaptureIntent.Save);

        await Create().CaptureRegionAsync(Token);

        Assert.Equal("2026-10-09_13-11-20", Assert.Single(_dialog.SuggestedNames));
        Assert.Equal((chosen, "login-error"), (Assert.Single(_storage.Saved).Folder, _storage.Saved[0].FileName));
        Assert.Empty(_clipboard.Images);
        Assert.Equal(chosen, _locations.SuggestedFolder);       // remembered for next time
        Assert.Equal(_locations.FallbackFolder, _locations.DefaultFolder); // but not the default
    }

    [Fact]
    public async Task Save_WithAlwaysSaveHere_MakesTheFolderTheNewDefault()
    {
        var chosen = _temp.Sub("Screens");
        _dialog.Answer = new SaveRequest(chosen, "shot", MakeDefault: true);
        SelectOnPrimary(CaptureIntent.Save);

        await Create().CaptureRegionAsync(Token);

        Assert.Equal(chosen, _locations.DefaultFolder);
    }

    [Fact]
    public async Task Save_DialogClosed_SavesNothing()
    {
        _dialog.Answer = null;
        SelectOnPrimary(CaptureIntent.Save);

        await Create().CaptureRegionAsync(Token);

        Assert.Empty(_storage.Saved);
        Assert.IsType<CaptureCompleted>(Assert.Single(_published));
        Assert.Empty(_feedback.Deliveries); // nothing to notify about
    }

    [Fact]
    public async Task Region_FreezesEveryDisplay()
    {
        SelectOnPrimary(CaptureIntent.Copy);

        await Create().CaptureRegionAsync(Token);

        Assert.Equal([Left.Bounds, Primary.Bounds], _screen.Captured);
    }

    [Fact]
    public async Task Region_Cancelled_DeliversNothing()
    {
        _selector.Choose = _ => null;

        await Create().CaptureRegionAsync(Token);

        Assert.Empty(_clipboard.Images);
        Assert.Empty(_storage.Saved);
        Assert.Empty(_published);
        Assert.Equal(0, _feedback.Taken);
    }

    [Fact]
    public async Task ActiveWindow_IsClippedToTheScreens()
    {
        // A maximized window's frame pokes 8 px beyond the primary display.
        _desktop.ForegroundWindow = new PixelBounds(-8, -8, 2576, 1456);

        await Create().CaptureActiveWindowAsync(Token);

        Assert.Equal(new PixelBounds(-8, 0, 2568, 1440), Assert.Single(_screen.Captured));
    }

    [Fact]
    public async Task ActiveWindow_WhenThereIsNone_DeliversNothing()
    {
        _desktop.ForegroundWindow = null;

        await Create().CaptureActiveWindowAsync(Token);

        Assert.Empty(_screen.Captured);
        Assert.Empty(_published);
    }

    [Fact]
    public async Task SecondCapture_WhileFirstIsSelecting_IsIgnored()
    {
        var release = new TaskCompletionSource<RegionSelection?>();
        var coordinator = Create(selector: new BlockingSelector(release.Task));

        var first = coordinator.CaptureRegionAsync(Token);
        await coordinator.CaptureFullScreenAsync(Token); // ignored: the region capture is still open
        release.SetResult(null);
        await first;

        Assert.Equal(2, _screen.Captured.Count); // only the region capture's two frozen displays
        Assert.Empty(_clipboard.Images);
    }

    private sealed class BlockingSelector(Task<RegionSelection?> result) : IRegionSelector
    {
        public Task<RegionSelection?> SelectAsync(IReadOnlyList<FrozenDisplay> displays, CancellationToken cancellationToken) => result;
    }
}
