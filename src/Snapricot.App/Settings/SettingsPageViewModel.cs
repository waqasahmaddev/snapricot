using Avalonia.Controls;
using Snapricot.Core.Modules;

namespace Snapricot.App.Settings;

public sealed record SettingsTabViewModel(string Title, Control Content);

/// <summary>Builds the Settings tabs from the module catalog: each tab stacks its modules' sections by order.</summary>
public sealed class SettingsPageViewModel(ModuleCatalog catalog, IServiceProvider services)
{
    public IReadOnlyList<SettingsTabViewModel> Tabs { get; } =
    [
        .. catalog.SettingsTabs.Select(tab =>
        {
            var panel = new StackPanel { Spacing = 22, Margin = new Avalonia.Thickness(26, 22) };
            foreach (var section in catalog.SectionsFor(tab.Id))
            {
                panel.Children.Add((Control)section.CreateView(services));
            }

            return new SettingsTabViewModel(tab.Title, new ScrollViewer { Content = panel });
        }),
    ];
}
