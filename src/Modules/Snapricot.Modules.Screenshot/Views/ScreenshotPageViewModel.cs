namespace Snapricot.Modules.Screenshot.Views;

public sealed record CaptureModeItem(string Title, string Description, string Gesture);

public sealed class ScreenshotPageViewModel
{
    public IReadOnlyList<CaptureModeItem> Modes { get; } =
    [
        new("Region", "Drag to select any part of the screen", "Ctrl+Shift+S"),
        new("Window", "Click a window to capture it", "Ctrl+Shift+W"),
        new("Full screen", "Capture the display under the cursor", "PrtSc"),
    ];

    public string Status => "Capture arrives in Phase 1. This page proves the module system works.";
}
