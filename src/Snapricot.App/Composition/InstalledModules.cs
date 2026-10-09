using Snapricot.Core.Modules;
using Snapricot.Modules.About;
using Snapricot.Modules.Screenshot;

namespace Snapricot.App.Composition;

/// <summary>
/// The explicit module list. Adding a feature = add a module project, reference it from Snapricot.App,
/// and add one line here. No reflection or assembly scanning, so trimming and Native AOT stay safe (ADR-0004).
/// </summary>
internal static class InstalledModules
{
    public static IReadOnlyList<ISnapricotModule> All { get; } =
    [
        new ShellModule(),
        new ScreenshotModule(),
        new AboutModule(),
    ];
}
