using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Feedback;

namespace Snapricot.Modules.Screenshot.Tests.Capture;

public sealed class ToastPlacementTests
{
    // A 1920x1040 work area (taskbar at the bottom) on a monitor placed left of the primary.
    private static readonly PixelBounds WorkArea = new(-1920, 0, 1920, 1040);

    [Theory]
    [InlineData(NotificationPosition.BottomRight, -1920 + 1920 - 340 - 12, 1040 - 80 - 12)]
    [InlineData(NotificationPosition.BottomLeft, -1920 + 12, 1040 - 80 - 12)]
    [InlineData(NotificationPosition.TopRight, -1920 + 1920 - 340 - 12, 12)]
    [InlineData(NotificationPosition.TopLeft, -1920 + 12, 12)]
    public void Origin_PutsTheToastInTheChosenCorner(NotificationPosition position, int x, int y)
    {
        Assert.Equal(new PixelPoint(x, y), ToastPlacement.Origin(WorkArea, 340, 80, position, 12));
    }

    [Fact]
    public void Default_IsBottomRight()
    {
        Assert.Equal(NotificationPosition.BottomRight, new Screenshot.Capture.ScreenshotSettings().NotificationPosition);
    }
}
