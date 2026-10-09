using Snapricot.Core.Capture;

namespace Snapricot.Core.Tests.Capture;

public sealed class PixelBoundsTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(99, 49, true)]
    [InlineData(100, 0, false)]
    [InlineData(0, 50, false)]
    [InlineData(-1, 0, false)]
    public void Contains_UsesHalfOpenEdges(int x, int y, bool expected)
    {
        var bounds = new PixelBounds(0, 0, 100, 50);

        Assert.Equal(expected, bounds.Contains(new PixelPoint(x, y)));
    }

    [Fact]
    public void Intersect_Overlapping_ReturnsOverlap()
    {
        var a = new PixelBounds(0, 0, 100, 100);
        var b = new PixelBounds(50, -20, 100, 60);

        Assert.Equal(new PixelBounds(50, 0, 50, 40), a.Intersect(b));
    }

    [Fact]
    public void Intersect_TouchingEdges_ReturnsNull()
    {
        var a = new PixelBounds(0, 0, 100, 100);
        var b = new PixelBounds(100, 0, 50, 50);

        Assert.Null(a.Intersect(b));
    }

    [Fact]
    public void Intersect_NegativeCoordinates_WorksForMonitorsLeftOfPrimary()
    {
        var leftMonitor = new PixelBounds(-1920, 0, 1920, 1080);
        var window = new PixelBounds(-100, 100, 400, 300);

        Assert.Equal(new PixelBounds(-100, 100, 100, 300), leftMonitor.Intersect(window));
    }

    [Theory]
    [InlineData(0, 10, true)]
    [InlineData(10, 0, true)]
    [InlineData(-5, 10, true)]
    [InlineData(1, 1, false)]
    public void IsEmpty_DetectsZeroOrNegativeSize(int width, int height, bool expected)
    {
        Assert.Equal(expected, new PixelBounds(0, 0, width, height).IsEmpty);
    }
}
