using System.Reflection;
using Snapricot.Core.Modules;
using Snapricot.Modules.About;
using Snapricot.Modules.Screenshot;

namespace Snapricot.Architecture.Tests;

public sealed class DependencyRulesTests
{
    // Register every module assembly here when you add a module.
    public static TheoryData<string> ModuleAssemblies =>
    [
        typeof(ScreenshotModule).Assembly.GetName().Name!,
        typeof(AboutModule).Assembly.GetName().Name!,
    ];

    private static readonly Assembly Core = typeof(ISnapricotModule).Assembly;

    private static string[] ReferencesOf(string assemblyName) =>
        [.. Assembly.Load(assemblyName).GetReferencedAssemblies().Select(a => a.Name!)];

    [Fact]
    public void Core_DoesNotReferenceUiFrameworks()
    {
        var references = ReferencesOf(Core.GetName().Name!);

        Assert.DoesNotContain(references, r => r.StartsWith("Avalonia", StringComparison.Ordinal));
        Assert.DoesNotContain(references, r => r.StartsWith("CommunityToolkit", StringComparison.Ordinal));
    }

    [Fact]
    public void Core_DoesNotReferenceAnySnapricotProject()
    {
        var references = ReferencesOf(Core.GetName().Name!);

        Assert.DoesNotContain(references, r => r.StartsWith("Snapricot.", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void Modules_DoNotReferencePlatformProjects(string module)
    {
        var references = ReferencesOf(module);

        Assert.DoesNotContain(references, r => r.StartsWith("Snapricot.Platform", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void Modules_DoNotReferenceOtherModules(string module)
    {
        var references = ReferencesOf(module);

        Assert.DoesNotContain(references, r => r.StartsWith("Snapricot.Modules.", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void Modules_DoNotReferenceTheApp(string module)
    {
        var references = ReferencesOf(module);

        Assert.DoesNotContain(references, r => r is "Snapricot" || r.StartsWith("Snapricot.App", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void Modules_ExposeExactlyOneModuleEntryPoint(string module)
    {
        var entryPoints = Assembly.Load(module).GetExportedTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISnapricotModule).IsAssignableFrom(t));

        Assert.Single(entryPoints);
    }
}
