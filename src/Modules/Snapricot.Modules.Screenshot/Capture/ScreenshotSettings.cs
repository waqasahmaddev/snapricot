using System.Text.Json.Serialization;

namespace Snapricot.Modules.Screenshot.Capture;

public enum ImageFormat
{
    Png,
    Jpg,
}

public static class ImageFormatExtensions
{
    public static string Extension(this ImageFormat format) => format == ImageFormat.Jpg ? ".jpg" : ".png";

    public static string DisplayName(this ImageFormat format) => format == ImageFormat.Jpg ? "JPG image" : "PNG image";
}

/// <summary>
/// Persisted Screenshot settings (section <c>"screenshot"</c> in settings.json). New properties must have defaults so
/// old settings files keep working (ADR-0012).
/// </summary>
public sealed record ScreenshotSettings
{
    /// <summary>The user's chosen default folder; <see langword="null"/> means <c>Pictures\Snapricot</c>.</summary>
    public string? DefaultFolder { get; set; }

    /// <summary>The last folder picked in the Save dialog, suggested next time.</summary>
    public string? LastFolder { get; set; }

    public string FileNamePattern { get; set; } = Core.Capture.FileNamePattern.Default;

    public ImageFormat Format { get; set; } = ImageFormat.Png;

    /// <summary>Copy also saves to the default folder, so every capture appears in History.</summary>
    public bool AutoSaveOnCopy { get; set; } = true;

    public bool OpenFolderAfterSave { get; set; }

    /// <summary>The small "Copied to clipboard" notification after a capture.</summary>
    public bool ShowNotification { get; set; } = true;

    public bool PlaySound { get; set; }

    /// <summary>Shows the pixel magnifier while drawing an area. Off by default (Settings → General).</summary>
    public bool ShowMagnifier { get; set; }

    public bool SaveTipDismissed { get; set; }
}

[JsonSerializable(typeof(ScreenshotSettings))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
internal sealed partial class ScreenshotSettingsJson : JsonSerializerContext;
