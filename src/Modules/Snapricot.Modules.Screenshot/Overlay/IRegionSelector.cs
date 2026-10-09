using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>A display together with the still image taken of it just before the overlay opened.</summary>
public sealed record FrozenDisplay(DisplayInfo Info, CapturedImage Image);

/// <summary>What the user chose in the overlay toolbar.</summary>
public enum CaptureIntent
{
    /// <summary>Copy button, Enter, or double-click: clipboard plus auto-save to the default folder.</summary>
    Copy,

    /// <summary>Save button: open the Save dialog to pick a name and folder.</summary>
    Save,
}

/// <summary>The user's selection: <paramref name="Bounds"/> is in the display image's pixel coordinates.</summary>
public sealed record RegionSelection(FrozenDisplay Display, PixelBounds Bounds, CaptureIntent Intent = CaptureIntent.Copy);

/// <summary>Shows the selection UI over frozen display images. Separate from the coordinator so capture logic is testable.</summary>
public interface IRegionSelector
{
    /// <summary>
    /// Shows the overlay and waits for the user; returns <see langword="null"/> when they cancel.
    /// <paramref name="onShown"/> runs once the overlay covers the screen (e.g. to hide the main window behind it).
    /// </summary>
    Task<RegionSelection?> SelectAsync(IReadOnlyList<FrozenDisplay> displays, Action? onShown, CancellationToken cancellationToken);
}
