using Microsoft.Extensions.DependencyInjection;

namespace Snapricot.Core.Modules;

/// <summary>The registration surface handed to <see cref="ISnapricotModule.Register"/>.</summary>
public interface IModuleBuilder
{
    /// <summary>Register the module's own services. Prefer singletons (ADR-0006).</summary>
    IServiceCollection Services { get; }

    IModuleBuilder AddNavItem(NavItem item);

    IModuleBuilder AddTrayItem(TrayItem item);

    IModuleBuilder AddHotkey(HotkeyBinding hotkey);

    IModuleBuilder AddSettingsTab(SettingsTab tab);

    IModuleBuilder AddSettingsSection(SettingsSection section);
}
