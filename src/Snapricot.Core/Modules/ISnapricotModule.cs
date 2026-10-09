namespace Snapricot.Core.Modules;

/// <summary>
/// A self-contained feature (Screenshot, OCR, Pin, ...). A module only describes what it adds;
/// the shell decides when and where to show it. See docs/architecture/overview.md and ADR-0004.
/// </summary>
public interface ISnapricotModule
{
    /// <summary>Stable, unique id in reverse-dot form, e.g. <c>snapricot.screenshot</c>.</summary>
    string Id { get; }

    string DisplayName { get; }

    /// <summary>Register services and UI contributions. Must be cheap: no I/O, no window creation.</summary>
    void Register(IModuleBuilder builder);
}
