namespace Snapricot.Core.Modules;

/// <summary>A sidebar entry. The view is created lazily the first time the user opens it.</summary>
/// <param name="IconPath">SVG path data (24x24 viewBox). Kept as a string so Core stays UI-agnostic.</param>
/// <param name="CreateView">Returns the page view; the shell hosts it. Called at most once per item.</param>
public sealed record NavItem(
    string Id,
    string Title,
    string IconPath,
    int Order,
    Func<IServiceProvider, object> CreateView);

/// <summary>An entry in the tray / menu-bar menu.</summary>
public sealed record TrayItem(
    string Id,
    string Title,
    int Order,
    Func<IServiceProvider, CancellationToken, Task> Execute);

/// <summary>A global hotkey with a user-overridable default gesture, e.g. <c>Ctrl+Shift+S</c>.</summary>
public sealed record HotkeyBinding(
    string Id,
    string Title,
    string DefaultGesture,
    Func<IServiceProvider, CancellationToken, Task> Execute);
