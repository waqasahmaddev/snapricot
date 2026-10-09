using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snapricot.Core.Events;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Modules;
using Snapricot.Core.Platform;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Views;

/// <summary>A capture tile: shows the current shortcut(s) from <see cref="HotkeyManager"/>, so rebinding updates it.</summary>
public sealed partial class CaptureModeItem(string bindingId, string title, string description, Geometry icon, bool isPrimary, ICommand capture)
    : ObservableObject
{
    public string BindingId { get; } = bindingId;

    public string Title { get; } = title;

    public string Description { get; } = description;

    public Geometry Icon { get; } = icon;

    public bool IsPrimary { get; } = isPrimary;

    public ICommand Capture { get; } = capture;

    [ObservableProperty]
    public partial string Gesture { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? AltGesture { get; set; }
}

public sealed record RecentCaptureItem(string Path, Bitmap Thumbnail, string Caption, ICommand Open);

/// <summary>Home screen (approved option A): capture tiles, then the newest captures from the default folder.</summary>
public sealed partial class ScreenshotPageViewModel : ObservableObject, IDisposable
{
    private const int RecentCount = 12;
    private const int ThumbnailWidth = 320;
    private static readonly TimeSpan ForegroundSettleDelay = TimeSpan.FromMilliseconds(120);

    private const string CropIcon = "M7,17V1H5V5H1V7H5V17A2,2 0 0,0 7,19H17V23H19V19H23V17M17,15H19V7C19,5.89 18.1,5 17,5H9V7H17V15Z";
    private const string WindowIcon = "M19,4C20.11,4 21,4.9 21,6V18A2,2 0 0,1 19,20H5C3.89,20 3,19.1 3,18V6A2,2 0 0,1 5,4H19M19,18V8H5V18H19Z";
    private const string MonitorIcon = "M21,16H3V4H21M21,2H3C1.89,2 1,2.89 1,4V16A2,2 0 0,0 3,18H10V20H8V22H16V20H14V18H21A2,2 0 0,0 23,16V4C23,2.89 22.1,2 21,2Z";

    private readonly ScreenshotPreferences _preferences;
    private readonly IShellService _shell;
    private readonly HotkeyManager _hotkeys;
    private readonly IDisposable _savedSubscription;
    private int _loadVersion;

    public ScreenshotPageViewModel(
        CaptureCoordinator coordinator,
        IEventBus events,
        ScreenshotPreferences preferences,
        IShellService shell,
        IShellWindow window,
        HotkeyManager hotkeys)
    {
        _preferences = preferences;
        _shell = shell;
        _hotkeys = hotkeys;

        // Started from the app, Snapricot must not be in the shot. Its window is first excluded from capture (it stays on
        // screen, so the mouse stays over Snapricot and no other app's cursor shows). Custom area then freezes, shows the
        // overlay, and hides the window behind it. Active window hides Snapricot so the previous app is active again.
        // Full screen just captures. The window comes back once the capture is finished.
        // AllowConcurrentExecutions keeps the tile from greying out while the capture runs.
        var options = AsyncRelayCommandOptions.AllowConcurrentExecutions;

        ICommand Region() => new AsyncRelayCommand(async () =>
        {
            await using var scope = await window.BeginCaptureAsync();
            await coordinator.CaptureRegionAsync(CancellationToken.None, onOverlayShown: scope.HideWindow);
        }, options);

        ICommand ActiveWindow() => new AsyncRelayCommand(async () =>
        {
            await using var scope = await window.BeginCaptureAsync();
            scope.HideWindow();
            await Task.Delay(ForegroundSettleDelay); // let Windows activate the app that was behind Snapricot
            await coordinator.CaptureActiveWindowAsync(CancellationToken.None);
        }, options);

        ICommand FullScreen() => new AsyncRelayCommand(async () =>
        {
            await using var scope = await window.BeginCaptureAsync();
            await coordinator.CaptureFullScreenAsync(CancellationToken.None);
        }, options);

        Modes =
        [
            new(CaptureCoordinator.RegionSource, "Custom area", "Drag, resize, then copy or save", StreamGeometry.Parse(CropIcon), true,
                Region()),
            new(CaptureCoordinator.WindowSource, "Active window", "The window you're working in", StreamGeometry.Parse(WindowIcon), false,
                ActiveWindow()),
            new(CaptureCoordinator.FullScreenSource, "Full screen", "The display under the cursor", StreamGeometry.Parse(MonitorIcon), false,
                FullScreen()),
        ];

        RefreshGestures();
        hotkeys.Changed += () => Dispatcher.UIThread.Post(RefreshGestures);
        preferences.Changed += () => Dispatcher.UIThread.Post(() => _ = LoadRecentAsync());
        _savedSubscription = events.Subscribe<CaptureSaved>((_, _) =>
        {
            Dispatcher.UIThread.Post(() => _ = LoadRecentAsync());
            return ValueTask.CompletedTask;
        });
        _ = LoadRecentAsync();
    }

    public IReadOnlyList<CaptureModeItem> Modes { get; }

    public ObservableCollection<RecentCaptureItem> Recent { get; } = [];

    [ObservableProperty]
    public partial bool HasRecent { get; set; }

    [RelayCommand]
    private void OpenFolder()
    {
        var folder = _preferences.DefaultFolder;
        Directory.CreateDirectory(folder);
        _shell.OpenFile(folder);
    }

    public void Dispose()
    {
        _savedSubscription.Dispose();
        DisposeThumbnails();
    }

    private void RefreshGestures()
    {
        var states = _hotkeys.States.ToDictionary(s => s.Binding.Id, s => s.Gesture);
        foreach (var mode in Modes)
        {
            mode.Gesture = states.GetValueOrDefault(mode.BindingId, string.Empty);
            mode.AltGesture = states.GetValueOrDefault(mode.BindingId + ScreenshotModule.AltSuffix);
        }
    }

    private async Task LoadRecentAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        var folder = _preferences.DefaultFolder;
        var now = DateTime.Now;

        // Decode small thumbnails off the UI thread; never load full-size images for a strip.
        var items = await Task.Run(() => RecentCaptures.Find(folder, RecentCount)
            .Select(file => TryThumbnail(file) is { } thumbnail
                ? new RecentCaptureItem(file.FullName, thumbnail, RecentCaptures.Caption(file.LastWriteTime, now, file.Extension),
                    new RelayCommand(() => _shell.OpenFile(file.FullName)))
                : null)
            .OfType<RecentCaptureItem>()
            .ToList());

        if (version != _loadVersion)
        {
            items.ForEach(i => i.Thumbnail.Dispose()); // a newer load superseded this one
            return;
        }

        DisposeThumbnails();
        Recent.Clear();
        items.ForEach(Recent.Add);
        HasRecent = Recent.Count > 0;
    }

    private static Bitmap? TryThumbnail(FileInfo file)
    {
        try
        {
            using var stream = file.OpenRead();
            return Bitmap.DecodeToWidth(stream, ThumbnailWidth, BitmapInterpolationMode.MediumQuality);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            return null; // a file being written, or not really an image
        }
    }

    private void DisposeThumbnails()
    {
        foreach (var item in Recent)
        {
            item.Thumbnail.Dispose();
        }
    }
}
