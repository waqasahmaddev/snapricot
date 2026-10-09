using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Modules;
using Snapricot.Modules.Screenshot.Views;

namespace Snapricot.Modules.Screenshot;

public sealed class ScreenshotModule : ISnapricotModule
{
    public const string ModuleId = "snapricot.screenshot";

    // Material Design "camera" icon, 24x24 viewBox.
    private const string CameraIcon =
        "M4,4H7L9,2H15L17,4H20A2,2 0 0,1 22,6V18A2,2 0 0,1 20,20H4A2,2 0 0,1 2,18V6A2,2 0 0,1 4,4" +
        "M12,7A5,5 0 0,0 7,12A5,5 0 0,0 12,17A5,5 0 0,0 17,12A5,5 0 0,0 12,7" +
        "M12,9A3,3 0 0,1 15,12A3,3 0 0,1 12,15A3,3 0 0,1 9,12A3,3 0 0,1 12,9Z";

    public string Id => ModuleId;

    public string DisplayName => "Screenshot";

    public void Register(IModuleBuilder builder)
    {
        builder.Services.AddSingleton<ScreenshotPageViewModel>();

        builder.AddNavItem(new NavItem(
            Id: $"{ModuleId}.page",
            Title: "Screenshot",
            IconPath: CameraIcon,
            Order: 100,
            CreateView: sp => new ScreenshotPage { DataContext = sp.GetRequiredService<ScreenshotPageViewModel>() }));
    }
}
