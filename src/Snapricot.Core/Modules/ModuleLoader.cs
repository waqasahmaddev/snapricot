using Microsoft.Extensions.DependencyInjection;

namespace Snapricot.Core.Modules;

/// <summary>
/// Runs every module's <see cref="ISnapricotModule.Register"/> and builds the <see cref="ModuleCatalog"/>.
/// Modules are passed explicitly (no assembly scanning) so the app stays trimming/AOT safe (ADR-0004).
/// </summary>
public static class ModuleLoader
{
    public static ModuleCatalog Load(IServiceCollection services, IEnumerable<ISnapricotModule> modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(modules);

        var builder = new ModuleBuilder(services);
        var infos = new List<ModuleInfo>();
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var module in modules)
        {
            if (!moduleIds.Add(module.Id))
            {
                throw new DuplicateContributionException("module", module.Id);
            }

            infos.Add(new ModuleInfo(module.Id, module.DisplayName));
            module.Register(builder);
        }

        var tabIds = builder.SettingsTabs.Select(t => t.Id).ToHashSet(StringComparer.Ordinal);
        if (builder.SettingsSections.FirstOrDefault(s => !tabIds.Contains(s.TabId)) is { } orphan)
        {
            throw new UnknownSettingsTabException(orphan.Id, orphan.TabId);
        }

        var catalog = new ModuleCatalog(
            infos,
            [.. builder.NavItems.OrderBy(i => i.Order).ThenBy(i => i.Title, StringComparer.Ordinal)],
            [.. builder.TrayItems.OrderBy(i => i.Order).ThenBy(i => i.Title, StringComparer.Ordinal)],
            [.. builder.Hotkeys],
            [.. builder.SettingsTabs.OrderBy(t => t.Order).ThenBy(t => t.Title, StringComparer.Ordinal)],
            [.. builder.SettingsSections.OrderBy(s => s.Order).ThenBy(s => s.Id, StringComparer.Ordinal)]);

        services.AddSingleton(catalog);
        return catalog;
    }

    private sealed class ModuleBuilder(IServiceCollection services) : IModuleBuilder
    {
        private readonly HashSet<string> _ids = new(StringComparer.Ordinal);

        public IServiceCollection Services { get; } = services;

        public List<NavItem> NavItems { get; } = [];

        public List<TrayItem> TrayItems { get; } = [];

        public List<HotkeyBinding> Hotkeys { get; } = [];

        public List<SettingsTab> SettingsTabs { get; } = [];

        public List<SettingsSection> SettingsSections { get; } = [];

        public IModuleBuilder AddNavItem(NavItem item) => Add(NavItems, item, "nav item", item.Id);

        public IModuleBuilder AddTrayItem(TrayItem item) => Add(TrayItems, item, "tray item", item.Id);

        public IModuleBuilder AddHotkey(HotkeyBinding hotkey) => Add(Hotkeys, hotkey, "hotkey", hotkey.Id);

        public IModuleBuilder AddSettingsTab(SettingsTab tab) => Add(SettingsTabs, tab, "settings tab", tab.Id);

        public IModuleBuilder AddSettingsSection(SettingsSection section) => Add(SettingsSections, section, "settings section", section.Id);

        private ModuleBuilder Add<T>(List<T> list, T item, string kind, string id)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (!_ids.Add($"{kind}:{id}"))
            {
                throw new DuplicateContributionException(kind, id);
            }

            list.Add(item);
            return this;
        }
    }
}
