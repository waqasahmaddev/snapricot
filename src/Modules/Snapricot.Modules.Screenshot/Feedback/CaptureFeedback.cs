using Avalonia.Threading;
using Snapricot.Core.Platform;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Feedback;

/// <summary>Shutter sound and the "Copied" notification, each controlled by its setting.</summary>
public sealed class CaptureFeedback(ScreenshotPreferences preferences, ISoundService sound, IShellService shell) : ICaptureFeedback
{
    private CaptureToast? _current;

    public void CaptureTaken()
    {
        if (preferences.Settings.PlaySound)
        {
            sound.PlayCaptureSound();
        }
    }

    public void Delivered(CaptureDelivery delivery)
    {
        if (!preferences.Settings.ShowNotification)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            _current?.Close(); // one notification at a time
            _current = new CaptureToast();
            _current.Closed += (sender, _) =>
            {
                if (ReferenceEquals(sender, _current))
                {
                    _current = null;
                }
            };
            _current.Show(delivery, delivery.SavedPath is { } path ? () => shell.RevealInFolder(path) : null);
        });
    }
}
