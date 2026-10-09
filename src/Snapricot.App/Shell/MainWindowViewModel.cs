using CommunityToolkit.Mvvm.ComponentModel;
using Snapricot.Core.Modules;

namespace Snapricot.App.Shell;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(ModuleCatalog catalog, IServiceProvider services)
    {
        NavItems = [.. catalog.NavItems.Select(item => new NavItemViewModel(item, services))];
        SelectedItem = NavItems.FirstOrDefault();
    }

    public IReadOnlyList<NavItemViewModel> NavItems { get; }

    public string VersionText { get; } = $"v{typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString(3)}";

    [ObservableProperty]
    public partial NavItemViewModel? SelectedItem { get; set; }

    public object? CurrentView => SelectedItem?.View;

    partial void OnSelectedItemChanged(NavItemViewModel? value) => OnPropertyChanged(nameof(CurrentView));
}
