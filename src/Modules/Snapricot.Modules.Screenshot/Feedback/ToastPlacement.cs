using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Feedback;

/// <summary>Where the capture notification appears (Settings → General). Default: bottom right.</summary>
public enum NotificationPosition
{
    BottomRight,
    BottomLeft,
    TopRight,
    TopLeft,
}

/// <summary>Pure placement math for the notification, in physical pixels (unit-tested).</summary>
public static class ToastPlacement
{
    public static PixelPoint Origin(PixelBounds workArea, int width, int height, NotificationPosition position, int margin)
    {
        var left = workArea.X + margin;
        var right = workArea.Right - width - margin;
        var top = workArea.Y + margin;
        var bottom = workArea.Bottom - height - margin;

        return position switch
        {
            NotificationPosition.BottomLeft => new PixelPoint(left, bottom),
            NotificationPosition.TopRight => new PixelPoint(right, top),
            NotificationPosition.TopLeft => new PixelPoint(left, top),
            _ => new PixelPoint(right, bottom),
        };
    }
}
