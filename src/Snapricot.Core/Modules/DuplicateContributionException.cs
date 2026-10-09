namespace Snapricot.Core.Modules;

/// <summary>Thrown at startup when two modules (or two contributions of one kind) share an id.</summary>
public sealed class DuplicateContributionException(string kind, string id)
    : InvalidOperationException($"A {kind} with id '{id}' is already registered.")
{
    public string Kind { get; } = kind;

    public string Id { get; } = id;
}

/// <summary>Thrown at startup when a settings section targets a tab no module registered.</summary>
public sealed class UnknownSettingsTabException(string sectionId, string tabId)
    : InvalidOperationException($"Settings section '{sectionId}' targets unknown tab '{tabId}'. Register the tab with AddSettingsTab.")
{
    public string SectionId { get; } = sectionId;

    public string TabId { get; } = tabId;
}
