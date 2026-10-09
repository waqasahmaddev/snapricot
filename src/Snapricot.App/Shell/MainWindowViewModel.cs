using CommunityToolkit.Mvvm.ComponentModel;
using Snapricot.Core.Modules;

namespace Snapricot.App.Shell;

/// <summary>The sidebar has two lists (main items and the bottom group); one selection spans both.</summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(ModuleCatalog catalog, IServiceProvider services)
    {
        TopItems = [.. catalog.NavItems.Where(i => i.Placement == NavPlacement.Top).Select(i => new NavItemViewModel(i, services))];
        BottomItems = [.. catalog.NavItems.Where(i => i.Placement == NavPlacement.Bottom).Select(i => new NavItemViewModel(i, services))];
        SelectedTopItem = TopItems.FirstOrDefault();
    }

    public IReadOnlyList<NavItemViewModel> TopItems { get; }

    public IReadOnlyList<NavItemViewModel> BottomItems { get; }

    public string VersionText { get; } = $"v{typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString(3)}";

    [ObservableProperty]
    public partial NavItemViewModel? SelectedTopItem { get; set; }

    [ObservableProperty]
    public partial NavItemViewModel? SelectedBottomItem { get; set; }

    public object? CurrentView => (SelectedTopItem ?? SelectedBottomItem)?.View;

    partial void OnSelectedTopItemChanged(NavItemViewModel? value)
    {
        if (value is not null)
        {
            SelectedBottomItem = null;
        }

        OnPropertyChanged(nameof(CurrentView));
    }

    partial void OnSelectedBottomItemChanged(NavItemViewModel? value)
    {
        if (value is not null)
        {
            SelectedTopItem = null;
        }

        OnPropertyChanged(nameof(CurrentView));
    }
}
