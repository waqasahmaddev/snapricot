using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapricot.Core.Modules;

namespace Snapricot.App.Shell;

/// <summary>Builds the tray / menu-bar icon from the module catalog plus the shell's own items.</summary>
internal static partial class TrayMenu
{
    public static void Install(Application app, ModuleCatalog catalog, IServiceProvider services, Action open, Action quit)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(TrayMenu));
        var menu = new NativeMenu();

        var openItem = new NativeMenuItem("Open Snapricot");
        openItem.Click += (_, _) => open();
        menu.Items.Add(openItem);

        if (catalog.TrayItems.Count > 0)
        {
            menu.Items.Add(new NativeMenuItemSeparator());
            foreach (var trayItem in catalog.TrayItems)
            {
                var menuItem = new NativeMenuItem(trayItem.Title);
                menuItem.Click += async (_, _) =>
                {
                    try
                    {
                        await trayItem.Execute(services, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        LogTrayItemFailed(logger, ex, trayItem.Id);
                    }
                };
                menu.Items.Add(menuItem);
            }
        }

        menu.Items.Add(new NativeMenuItemSeparator());
        var quitItem = new NativeMenuItem("Quit");
        quitItem.Click += (_, _) => quit();
        menu.Items.Add(quitItem);

        using var iconStream = AssetLoader.Open(new Uri("avares://Snapricot/Assets/snapricot-tray.png"));
        var tray = new TrayIcon
        {
            Icon = new WindowIcon(iconStream),
            ToolTipText = "Snapricot",
            Menu = menu,
        };
        tray.Clicked += (_, _) => open();

        TrayIcon.SetIcons(app, [tray]);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Tray item {TrayItemId} failed")]
    private static partial void LogTrayItemFailed(ILogger logger, Exception exception, string trayItemId);
}
