namespace Snapricot.Core.Modules;

/// <summary>Where a sidebar entry sits: the main list, or the bottom group (Settings, About).</summary>
public enum NavPlacement
{
    Top,
    Bottom,
}

/// <summary>A sidebar entry. The view is created lazily the first time the user opens it.</summary>
/// <param name="IconPath">SVG path data (24x24 viewBox). Kept as a string so Core stays UI-agnostic.</param>
/// <param name="CreateView">Returns the page view; the shell hosts it. Called at most once per item.</param>
public sealed record NavItem(
    string Id,
    string Title,
    string IconPath,
    int Order,
    Func<IServiceProvider, object> CreateView,
    NavPlacement Placement = NavPlacement.Top);

/// <summary>An entry in the tray / menu-bar menu.</summary>
public sealed record TrayItem(
    string Id,
    string Title,
    int Order,
    Func<IServiceProvider, CancellationToken, Task> Execute);

/// <summary>A global hotkey with a user-overridable default gesture, e.g. <c>Ctrl+PrtSc</c>.</summary>
public sealed record HotkeyBinding(
    string Id,
    string Title,
    string DefaultGesture,
    Func<IServiceProvider, CancellationToken, Task> Execute);

/// <summary>A tab on the Settings page. Well-known ids are in <see cref="SettingsTabs"/>.</summary>
public sealed record SettingsTab(string Id, string Title, int Order);

/// <summary>A block of settings UI placed on a tab; sections on a tab are stacked by <see cref="Order"/>.</summary>
public sealed record SettingsSection(
    string TabId,
    string Id,
    int Order,
    Func<IServiceProvider, object> CreateView);

/// <summary>Ids of the shell's built-in Settings tabs, so modules can add sections to them.</summary>
public static class SettingsTabs
{
    public const string General = "general";
    public const string SaveTo = "save-to";
    public const string Hotkeys = "hotkeys";
}
