using Avalonia.Threading;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>Shows <see cref="SaveCaptureDialog"/> on the UI thread and waits for it to close.</summary>
public sealed class AvaloniaSaveDialog(ScreenshotPreferences locations) : ISaveDialog
{
    public async Task<SaveRequest?> AskAsync(CapturedImage image, string suggestedFileName, CancellationToken cancellationToken)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => AskAsync(image, suggestedFileName, cancellationToken));
        }

        var dialog = new SaveCaptureDialog();
        dialog.Initialize(image, suggestedFileName, locations);

        var closed = new TaskCompletionSource<SaveRequest?>(TaskCreationOptions.RunContinuationsAsynchronously);
        dialog.Closed += (_, _) => closed.TrySetResult(dialog.Result);
        await using var registration = cancellationToken.Register(() => Dispatcher.UIThread.Post(dialog.Close));

        // No owner window: the main window may be hidden in the tray. The dialog is topmost instead.
        dialog.Show();
        dialog.Activate();
        return await closed.Task;
    }
}
