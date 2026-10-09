using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>What the user confirmed in the Save dialog.</summary>
/// <param name="MakeDefault">"Always save here" was ticked.</param>
public sealed record SaveRequest(string Folder, string FileName, bool MakeDefault);

/// <summary>The Save dialog. An interface so the save flow is testable without UI.</summary>
public interface ISaveDialog
{
    /// <summary>Returns <see langword="null"/> when the user closes the dialog without saving.</summary>
    Task<SaveRequest?> AskAsync(CapturedImage image, string suggestedFileName, CancellationToken cancellationToken);
}
