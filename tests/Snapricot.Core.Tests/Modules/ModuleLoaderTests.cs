using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Modules;

namespace Snapricot.Core.Tests.Modules;

public sealed class ModuleLoaderTests
{
    private sealed class TestModule(string id, Action<IModuleBuilder> register) : ISnapricotModule
    {
        public string Id => id;

        public string DisplayName => id;

        public void Register(IModuleBuilder builder) => register(builder);
    }

    private sealed class MarkerService;

    private static NavItem Nav(string id, int order, string title = "Title") =>
        new(id, title, "M0,0Z", order, _ => new object());

    private static TrayItem Tray(string id, int order) =>
        new(id, id, order, (_, _) => Task.CompletedTask);

    [Fact]
    public void Load_CollectsModulesAndContributions()
    {
        var services = new ServiceCollection();
        var module = new TestModule("test.a", b => b
            .AddNavItem(Nav("a.page", 1))
            .AddTrayItem(Tray("a.tray", 1))
            .AddHotkey(new HotkeyBinding("a.key", "Key", "Ctrl+K", (_, _) => Task.CompletedTask)));

        var catalog = ModuleLoader.Load(services, [module]);

        Assert.Equal("test.a", Assert.Single(catalog.Modules).Id);
        Assert.Equal("a.page", Assert.Single(catalog.NavItems).Id);
        Assert.Equal("a.tray", Assert.Single(catalog.TrayItems).Id);
        Assert.Equal("a.key", Assert.Single(catalog.Hotkeys).Id);
    }

    [Fact]
    public void Load_SortsNavAndTrayItemsByOrderThenTitle()
    {
        var services = new ServiceCollection();
        var first = new TestModule("test.a", b => b.AddNavItem(Nav("late", 200)).AddTrayItem(Tray("t2", 2)));
        var second = new TestModule("test.b", b => b
            .AddNavItem(Nav("early-b", 100, "B"))
            .AddNavItem(Nav("early-a", 100, "A"))
            .AddTrayItem(Tray("t1", 1)));

        var catalog = ModuleLoader.Load(services, [first, second]);

        Assert.Equal(["early-a", "early-b", "late"], catalog.NavItems.Select(i => i.Id));
        Assert.Equal(["t1", "t2"], catalog.TrayItems.Select(i => i.Id));
    }

    [Fact]
    public void Load_RegistersModuleServicesAndCatalog()
    {
        var services = new ServiceCollection();
        var module = new TestModule("test.a", b => b.Services.AddSingleton<MarkerService>());

        var catalog = ModuleLoader.Load(services, [module]);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<MarkerService>());
        Assert.Same(catalog, provider.GetRequiredService<ModuleCatalog>());
    }

    [Fact]
    public void Load_WithDuplicateModuleId_Throws()
    {
        var modules = new[] { new TestModule("dup", _ => { }), new TestModule("dup", _ => { }) };

        var ex = Assert.Throws<DuplicateContributionException>(() => ModuleLoader.Load(new ServiceCollection(), modules));

        Assert.Equal("module", ex.Kind);
        Assert.Equal("dup", ex.Id);
    }

    [Fact]
    public void Load_WithDuplicateNavItemAcrossModules_Throws()
    {
        var modules = new[]
        {
            new TestModule("test.a", b => b.AddNavItem(Nav("same", 1))),
            new TestModule("test.b", b => b.AddNavItem(Nav("same", 2))),
        };

        var ex = Assert.Throws<DuplicateContributionException>(() => ModuleLoader.Load(new ServiceCollection(), modules));

        Assert.Equal("nav item", ex.Kind);
    }

    [Fact]
    public void Load_AllowsSameIdForDifferentContributionKinds()
    {
        var module = new TestModule("test.a", b => b.AddNavItem(Nav("shared", 1)).AddTrayItem(Tray("shared", 1)));

        var catalog = ModuleLoader.Load(new ServiceCollection(), [module]);

        Assert.Single(catalog.NavItems);
        Assert.Single(catalog.TrayItems);
    }

    [Fact]
    public void Load_CollectsSettingsTabsAndSections_SortedByOrder()
    {
        var module = new TestModule("test.a", b => b
            .AddSettingsTab(new SettingsTab("hotkeys", "Hotkeys", 800))
            .AddSettingsTab(new SettingsTab("general", "General", 0))
            .AddSettingsSection(new SettingsSection("general", "late", 200, _ => new object()))
            .AddSettingsSection(new SettingsSection("general", "early", 100, _ => new object()))
            .AddSettingsSection(new SettingsSection("hotkeys", "keys", 100, _ => new object())));

        var catalog = ModuleLoader.Load(new ServiceCollection(), [module]);

        Assert.Equal(["general", "hotkeys"], catalog.SettingsTabs.Select(t => t.Id));
        Assert.Equal(["early", "late"], catalog.SectionsFor("general").Select(s => s.Id));
    }

    [Fact]
    public void Load_SectionsMayTargetATabFromAnotherModule()
    {
        var shell = new TestModule("shell", b => b.AddSettingsTab(new SettingsTab("general", "General", 0)));
        var feature = new TestModule("feature", b => b.AddSettingsSection(new SettingsSection("general", "feature.section", 100, _ => new object())));

        var catalog = ModuleLoader.Load(new ServiceCollection(), [feature, shell]);

        Assert.Single(catalog.SectionsFor("general"));
    }

    [Fact]
    public void Load_SectionForUnknownTab_Throws()
    {
        var module = new TestModule("test.a", b => b.AddSettingsSection(new SettingsSection("nowhere", "lost", 1, _ => new object())));

        var ex = Assert.Throws<UnknownSettingsTabException>(() => ModuleLoader.Load(new ServiceCollection(), [module]));

        Assert.Equal(("lost", "nowhere"), (ex.SectionId, ex.TabId));
    }

    [Fact]
    public void NavItem_DefaultsToTopPlacement()
    {
        Assert.Equal(NavPlacement.Top, Nav("x", 1).Placement);
    }
}
