using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Modules;
using Snapricot.Core.Settings;
using Snapricot.Modules.Screenshot.Capture;
using Snapricot.Modules.Screenshot.Feedback;
using Snapricot.Modules.Screenshot.Overlay;
using Snapricot.Modules.Screenshot.Settings;
using Snapricot.Modules.Screenshot.Views;

namespace Snapricot.Modules.Screenshot;

public sealed class ScreenshotModule : ISnapricotModule
{
    public const string ModuleId = "snapricot.screenshot";

    // Default hotkeys use PrintScreen combos so they never steal common app shortcuts (ADR-0011).
    public const string RegionGesture = "Ctrl+PrtSc";
    public const string WindowGesture = "Alt+PrtSc";
    public const string FullScreenGesture = "Shift+PrtSc";

    // Material Design "camera" icon, 24x24 viewBox.
    private const string CameraIcon =
        "M4,4H7L9,2H15L17,4H20A2,2 0 0,1 22,6V18A2,2 0 0,1 20,20H4A2,2 0 0,1 2,18V6A2,2 0 0,1 4,4" +
        "M12,7A5,5 0 0,0 7,12A5,5 0 0,0 12,17A5,5 0 0,0 17,12A5,5 0 0,0 12,7" +
        "M12,9A3,3 0 0,1 15,12A3,3 0 0,1 12,15A3,3 0 0,1 9,12A3,3 0 0,1 12,9Z";

    public string Id => ModuleId;

    public string DisplayName => "Screenshot";

    public void Register(IModuleBuilder builder)
    {
        builder.Services.AddSingleton(sp => new ScreenshotPreferences(sp.GetRequiredService<ISettingsStore>()));
        builder.Services.AddSingleton<ICaptureStorage, ImageCaptureStorage>();
        builder.Services.AddSingleton<ISaveDialog, AvaloniaSaveDialog>();
        builder.Services.AddSingleton<ICaptureFeedback, CaptureFeedback>();
        builder.Services.AddSingleton<IRegionSelector, OverlayRegionSelector>();
        builder.Services.AddSingleton<CaptureCoordinator>();
        builder.Services.AddSingleton<ScreenshotPageViewModel>();
        builder.Services.AddTransient<CaptureSettingsViewModel>();
        builder.Services.AddTransient<SaveToSettingsViewModel>();

        builder
            .AddNavItem(new NavItem(
                Id: $"{ModuleId}.page",
                Title: "Screenshot",
                IconPath: CameraIcon,
                Order: 100,
                CreateView: sp => new ScreenshotPage { DataContext = sp.GetRequiredService<ScreenshotPageViewModel>() }))
            .AddTrayItem(new TrayItem($"{ModuleId}.tray.region", "Capture region", 100, Region))
            .AddTrayItem(new TrayItem($"{ModuleId}.tray.window", "Capture active window", 110, Window))
            .AddTrayItem(new TrayItem($"{ModuleId}.tray.fullscreen", "Capture full screen", 120, FullScreen))
            .AddHotkey(new HotkeyBinding(CaptureCoordinator.RegionSource, "Capture custom area", RegionGesture, Region))
            .AddHotkey(new HotkeyBinding(CaptureCoordinator.WindowSource, "Capture active window", WindowGesture, Window))
            .AddHotkey(new HotkeyBinding(CaptureCoordinator.FullScreenSource, "Capture full screen", FullScreenGesture, FullScreen))
            .AddSettingsTab(new SettingsTab(SettingsTabs.SaveTo, "Save to", 100))
            .AddSettingsSection(new SettingsSection(SettingsTabs.SaveTo, $"{ModuleId}.settings.save-to", 100,
                sp => new SaveToSettingsView { DataContext = sp.GetRequiredService<SaveToSettingsViewModel>() }))
            .AddSettingsSection(new SettingsSection(SettingsTabs.General, $"{ModuleId}.settings.capture", 200,
                sp => new CaptureSettingsView { DataContext = sp.GetRequiredService<CaptureSettingsViewModel>() }));
    }

    private static Task Region(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<CaptureCoordinator>().CaptureRegionAsync(cancellationToken);

    private static Task Window(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<CaptureCoordinator>().CaptureActiveWindowAsync(cancellationToken);

    private static Task FullScreen(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<CaptureCoordinator>().CaptureFullScreenAsync(cancellationToken);
}
