using Avalonia.Media;
using Snapricot.Core.Modules;

namespace Snapricot.App.Shell;

public sealed class NavItemViewModel(NavItem item, IServiceProvider services)
{
    private object? _view;

    public string Title => item.Title;

    public Geometry Icon { get; } = StreamGeometry.Parse(item.IconPath);

    /// <summary>Created on first access, so a page that is never opened costs nothing.</summary>
    public object View => _view ??= item.CreateView(services);
}
