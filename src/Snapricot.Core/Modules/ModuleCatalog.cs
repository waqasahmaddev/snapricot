namespace Snapricot.Core.Modules;

public sealed record ModuleInfo(string Id, string DisplayName);

/// <summary>Everything the loaded modules contributed, already sorted for display.</summary>
public sealed class ModuleCatalog(
    IReadOnlyList<ModuleInfo> modules,
    IReadOnlyList<NavItem> navItems,
    IReadOnlyList<TrayItem> trayItems,
    IReadOnlyList<HotkeyBinding> hotkeys,
    IReadOnlyList<SettingsTab> settingsTabs,
    IReadOnlyList<SettingsSection> settingsSections)
{
    public IReadOnlyList<ModuleInfo> Modules { get; } = modules;

    public IReadOnlyList<NavItem> NavItems { get; } = navItems;

    public IReadOnlyList<TrayItem> TrayItems { get; } = trayItems;

    public IReadOnlyList<HotkeyBinding> Hotkeys { get; } = hotkeys;

    public IReadOnlyList<SettingsTab> SettingsTabs { get; } = settingsTabs;

    public IReadOnlyList<SettingsSection> SettingsSections { get; } = settingsSections;

    public IEnumerable<SettingsSection> SectionsFor(string tabId) =>
        SettingsSections.Where(section => section.TabId == tabId);
}
