using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Snapricot.Core.Capture;
using Snapricot.Core.Events;
using Snapricot.Core.Platform;
using Snapricot.Modules.Screenshot.Overlay;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>
/// Runs a capture end to end. <see cref="CaptureIntent.Copy"/>: clipboard first (fast), then (if enabled) auto-save to the
/// default folder. <see cref="CaptureIntent.Save"/>: the Save dialog picks the name and folder. Then optional reveal-in-folder
/// and the sound/notification feedback. Publishes
/// <see cref="CaptureCompleted"/> once pixels exist and <see cref="CaptureSaved"/> once a file is written.
/// Only one capture (up to delivery) runs at a time.
/// </summary>
public sealed partial class CaptureCoordinator(
    IScreenCapture screen,
    IDesktopService desktop,
    IClipboardService clipboard,
    IRegionSelector regionSelector,
    ISaveDialog saveDialog,
    ICaptureStorage storage,
    ScreenshotPreferences preferences,
    ICaptureFeedback feedback,
    IShellService shell,
    IEventBus events,
    ILogger<CaptureCoordinator> logger)
{
    public const string RegionSource = "snapricot.screenshot.region";
    public const string WindowSource = "snapricot.screenshot.window";
    public const string FullScreenSource = "snapricot.screenshot.fullscreen";

    private int _busy;

    /// <summary>
    /// Freezes every display, then shows the overlay over the frozen images and lets the user draw and adjust a custom
    /// area. <paramref name="onOverlayShown"/> runs once the overlay covers the screen (the app uses it to hide its window
    /// behind the overlay, so the user never sees it go and the mouse never passes over another app).
    /// </summary>
    public Task CaptureRegionAsync(CancellationToken cancellationToken, Action? onOverlayShown = null) =>
        RunAsync(RegionSource, async () =>
        {
            var displays = screen.GetDisplays();
            var frozen = await Task.WhenAll(displays.Select(async display =>
                new FrozenDisplay(display, await screen.CaptureAsync(display.Bounds, cancellationToken).ConfigureAwait(false))));

            var selection = await regionSelector.SelectAsync(frozen, onOverlayShown, cancellationToken);
            return selection is null
                ? null
                : (ImageOperations.Crop(selection.Display.Image, selection.Bounds), selection.Intent);
        }, cancellationToken);

    /// <summary>Captures the whole display under the mouse cursor.</summary>
    public Task CaptureFullScreenAsync(CancellationToken cancellationToken) =>
        RunAsync(FullScreenSource, async () =>
        {
            var displays = screen.GetDisplays();
            if (displays.Count == 0)
            {
                return null;
            }

            var cursor = desktop.GetCursorPosition();
            var display = displays.FirstOrDefault(d => d.Bounds.Contains(cursor))
                ?? displays.FirstOrDefault(d => d.IsPrimary)
                ?? displays[0];
            return (await screen.CaptureAsync(display.Bounds, cancellationToken).ConfigureAwait(false), CaptureIntent.Copy);
        }, cancellationToken);

    /// <summary>Captures the visible frame of the focused window, clipped to the screens.</summary>
    public Task CaptureActiveWindowAsync(CancellationToken cancellationToken) =>
        RunAsync(WindowSource, async () =>
        {
            if (desktop.GetForegroundWindowBounds() is not { } window)
            {
                LogNoForegroundWindow();
                return null;
            }

            // Union of the window's visible parts on each display (a maximized window pokes a few pixels off-screen).
            var visible = screen.GetDisplays()
                .Select(d => d.Bounds.Intersect(window))
                .OfType<PixelBounds>()
                .ToList();
            if (visible.Count == 0)
            {
                return null;
            }

            var left = visible.Min(b => b.X);
            var top = visible.Min(b => b.Y);
            var bounds = new PixelBounds(left, top, visible.Max(b => b.Right) - left, visible.Max(b => b.Bottom) - top);
            return (await screen.CaptureAsync(bounds, cancellationToken).ConfigureAwait(false), CaptureIntent.Copy);
        }, cancellationToken);

    private async Task RunAsync(
        string sourceId,
        Func<Task<(CapturedImage Image, CaptureIntent Intent)?>> capture,
        CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _busy, 1) == 1)
        {
            LogAlreadyCapturing(sourceId);
            return;
        }

        (CapturedImage Image, CaptureIntent Intent) result;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (await capture() is not { } captured)
            {
                LogCancelled(sourceId);
                return;
            }

            result = captured;
            feedback.CaptureTaken();
            if (result.Intent == CaptureIntent.Copy)
            {
                await clipboard.SetImageAsync(result.Image, cancellationToken);
            }

            LogCaptured(sourceId, result.Image.Width, result.Image.Height, stopwatch.ElapsedMilliseconds);
            await events.PublishAsync(new CaptureCompleted(result.Image, sourceId), cancellationToken);
        }
        finally
        {
            // Saving (and the Save dialog) happen outside the busy window, so a new capture is never blocked by disk or a dialog.
            Interlocked.Exchange(ref _busy, 0);
        }

        var image = result.Image;
        var settings = preferences.Settings;
        var defaultName = FileNamePattern.Format(settings.FileNamePattern, image.CapturedAt);
        string? savedPath = null;

        if (result.Intent == CaptureIntent.Save)
        {
            if (await saveDialog.AskAsync(image, defaultName, cancellationToken) is not { } request)
            {
                LogSaveDialogClosed();
                return;
            }

            savedPath = await SaveAsync(image, request.Folder, request.FileName, settings.Format, cancellationToken);
            preferences.Remember(request.Folder, request.MakeDefault);
        }
        else if (settings.AutoSaveOnCopy)
        {
            if (preferences.DefaultIsMissing)
            {
                LogDefaultFolderMissing(preferences.FallbackFolder);
            }

            savedPath = await SaveAsync(image, preferences.DefaultFolder, defaultName, settings.Format, cancellationToken);
        }

        if (savedPath is not null && settings.OpenFolderAfterSave)
        {
            shell.RevealInFolder(savedPath);
        }

        feedback.Delivered(new CaptureDelivery(image, Copied: result.Intent == CaptureIntent.Copy, savedPath));
    }

    private async Task<string> SaveAsync(CapturedImage image, string folder, string fileName, ImageFormat format, CancellationToken cancellationToken)
    {
        var path = await storage.SaveAsync(image, folder, fileName, format, cancellationToken);
        LogSaved(path);
        await events.PublishAsync(new CaptureSaved(image, path), cancellationToken);
        return path;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Captured {SourceId}: {Width}x{Height} in {ElapsedMs} ms")]
    private partial void LogCaptured(string sourceId, int width, int height, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Capture {SourceId} cancelled")]
    private partial void LogCancelled(string sourceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Ignored {SourceId}: a capture is already running")]
    private partial void LogAlreadyCapturing(string sourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saved capture to {Path}")]
    private partial void LogSaved(string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Save dialog closed without saving")]
    private partial void LogSaveDialogClosed();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Default save folder is unavailable; saving to {FallbackFolder} instead")]
    private partial void LogDefaultFolderMissing(string fallbackFolder);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No foreground window to capture")]
    private partial void LogNoForegroundWindow();
}
