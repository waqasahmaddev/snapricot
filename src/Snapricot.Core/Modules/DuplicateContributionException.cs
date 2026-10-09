namespace Snapricot.Core.Modules;

/// <summary>Thrown at startup when two modules (or two contributions of one kind) share an id.</summary>
public sealed class DuplicateContributionException(string kind, string id)
    : InvalidOperationException($"A {kind} with id '{id}' is already registered.")
{
    public string Kind { get; } = kind;

    public string Id { get; } = id;
}
