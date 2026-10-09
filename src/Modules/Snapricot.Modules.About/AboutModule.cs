using Snapricot.Core.Modules;
using Snapricot.Modules.About.Views;

namespace Snapricot.Modules.About;

public sealed class AboutModule : ISnapricotModule
{
    public const string ModuleId = "snapricot.about";

    // Material Design "information-outline" icon, 24x24 viewBox.
    private const string InfoIcon =
        "M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20" +
        "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z";

    public string Id => ModuleId;

    public string DisplayName => "About";

    public void Register(IModuleBuilder builder) =>
        builder.AddNavItem(new NavItem(
            Id: $"{ModuleId}.page",
            Title: "About",
            IconPath: InfoIcon,
            Order: 1000,
            CreateView: _ => new AboutPage()));
}
