using Avalonia;
using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Overlay;

namespace Snapricot.Modules.Screenshot.Tests.Overlay;

public sealed class SelectionGeometryTests
{
    private static readonly Size Area = new(1000, 800);
    private static readonly Rect Box = new(100, 100, 200, 100);

    [Fact]
    public void Normalize_DragUpAndLeft_ReturnsPositiveRect()
    {
        Assert.Equal(new Rect(40, 20, 60, 60), SelectionGeometry.Normalize(new Point(100, 80), new Point(40, 20)));
    }

    [Fact]
    public void Handles_AreTheFourCornersAndFourEdgeMidpoints()
    {
        var centers = SelectionGeometry.Handles(Box).ToDictionary(h => h.Edges, h => h.Center);

        Assert.Equal(8, centers.Count);
        Assert.Equal(new Point(100, 100), centers[HandleEdges.Left | HandleEdges.Top]);
        Assert.Equal(new Point(200, 100), centers[HandleEdges.Top]);
        Assert.Equal(new Point(300, 150), centers[HandleEdges.Right]);
        Assert.Equal(new Point(300, 200), centers[HandleEdges.Right | HandleEdges.Bottom]);
    }

    [Theory]
    [InlineData(102, 98, HandleEdges.Left | HandleEdges.Top, false)]
    [InlineData(200, 203, HandleEdges.Bottom, false)]
    [InlineData(296, 150, HandleEdges.Right, false)]
    [InlineData(150, 150, HandleEdges.None, true)]
    [InlineData(50, 50, HandleEdges.None, false)]
    public void HitTest_PrefersHandlesThenInside(double x, double y, HandleEdges edges, bool inside)
    {
        Assert.Equal(new SelectionHit(edges, inside), SelectionGeometry.HitTest(Box, new Point(x, y), 16));
    }

    [Fact]
    public void Resize_BottomRightCorner_GrowsWidthAndHeight()
    {
        Assert.Equal(new Rect(100, 100, 250, 140),
            SelectionGeometry.Resize(Box, HandleEdges.Right | HandleEdges.Bottom, new Vector(50, 40), null, 4, Area));
    }

    [Fact]
    public void Resize_LeftEdge_MovesOnlyTheLeft()
    {
        Assert.Equal(new Rect(60, 100, 240, 100), SelectionGeometry.Resize(Box, HandleEdges.Left, new Vector(-40, 999), null, 4, Area));
    }

    [Fact]
    public void Resize_PastTheOppositeEdge_StopsAtMinimumSize()
    {
        Assert.Equal(new Rect(296, 100, 4, 100), SelectionGeometry.Resize(Box, HandleEdges.Left, new Vector(500, 0), null, 4, Area));
    }

    [Fact]
    public void Resize_BeyondTheScreen_IsClamped()
    {
        Assert.Equal(new Rect(100, 100, 900, 100), SelectionGeometry.Resize(Box, HandleEdges.Right, new Vector(5000, 0), null, 4, Area));
    }

    [Fact]
    public void Resize_WithAspect_WidthDrivesHeight()
    {
        var resized = SelectionGeometry.Resize(Box, HandleEdges.Right | HandleEdges.Bottom, new Vector(120, 0), 16d / 9, 4, Area);

        Assert.Equal(new Rect(100, 100, 320, 180), resized);
    }

    [Fact]
    public void Resize_TopLeftWithAspect_KeepsBottomRightFixed()
    {
        var box = new Rect(300, 300, 200, 100);

        var resized = SelectionGeometry.Resize(box, HandleEdges.Left | HandleEdges.Top, new Vector(-100, 0), 1, 4, Area);

        Assert.Equal(new Rect(200, 100, 300, 300), resized);
    }

    [Fact]
    public void FitAspect_ShrinksToStayOnScreen()
    {
        var fitted = SelectionGeometry.FitAspect(new Rect(800, 700, 150, 10), HandleEdges.Right | HandleEdges.Bottom, 1, Area);

        Assert.Equal(new Rect(800, 700, 100, 100), fitted);
    }

    [Fact]
    public void DragEdges_FollowsDragDirection()
    {
        Assert.Equal(HandleEdges.Left | HandleEdges.Bottom, SelectionGeometry.DragEdges(new Point(100, 100), new Point(50, 150)));
    }

    [Fact]
    public void Move_IsClampedToTheScreen()
    {
        Assert.Equal(new Rect(800, 0, 200, 100), SelectionGeometry.Move(Box, new Vector(5000, -5000), Area));
    }

    [Fact]
    public void WithPixelSize_ConvertsAndKeepsTopLeft()
    {
        Assert.Equal(new Rect(100, 100, 640, 360), SelectionGeometry.WithPixelSize(Box, 960, 540, 1.5, Area));
    }

    [Fact]
    public void WithPixelSize_TooBigForTheSpot_ShiftsLeftAndUp()
    {
        Assert.Equal(new Rect(200, 100, 800, 100), SelectionGeometry.WithPixelSize(new Rect(500, 100, 10, 10), 800, 100, 1, Area));
    }

    [Fact]
    public void ToPixels_AtScale150_ScalesToPhysicalPixels()
    {
        Assert.Equal(new PixelBounds(15, 30, 45, 60), SelectionGeometry.ToPixels(new Rect(10, 20, 30, 40), 1.5, 2880, 1620));
    }

    [Fact]
    public void ToPixels_FractionalEdges_GrowOutwardSoNothingSelectedIsLost()
    {
        // 10.4..20.6 DIPs at 1.25 => 13.0..25.75 px => 13..26
        Assert.Equal(new PixelBounds(13, 13, 13, 13), SelectionGeometry.ToPixels(new Rect(10.4, 10.4, 10.2, 10.2), 1.25, 1000, 1000));
    }

    [Fact]
    public void ToPixels_BeyondImage_IsClamped_OrNullWhenNothingIsLeft()
    {
        Assert.Equal(new PixelBounds(90, 0, 10, 10), SelectionGeometry.ToPixels(new Rect(90, -5, 50, 15), 1, 100, 100));
        Assert.Null(SelectionGeometry.ToPixels(new Rect(200, 200, 10, 10), 1, 100, 100));
    }

    [Theory]
    [InlineData(100, 100, 124, 124)] // room below-right
    [InlineData(950, 100, 806, 124)] // flips left near the right edge
    [InlineData(100, 700, 124, 556)] // flips up near the bottom edge
    public void MagnifierOrigin_StaysOnScreen(double x, double y, double expectedX, double expectedY)
    {
        Assert.Equal(new Point(expectedX, expectedY), SelectionGeometry.MagnifierOrigin(new Point(x, y), Area, 120, 24));
    }

    [Fact]
    public void ToolbarOrigin_DefaultsToBelowRightAligned()
    {
        Assert.Equal(new Point(150, 208), SelectionGeometry.ToolbarOrigin(Box, new Size(150, 40), Area));
    }

    [Fact]
    public void ToolbarOrigin_NoRoomBelow_GoesAbove()
    {
        Assert.Equal(new Point(150, 652), SelectionGeometry.ToolbarOrigin(new Rect(100, 700, 200, 90), new Size(150, 40), Area));
    }

    [Fact]
    public void ToolbarOrigin_FullScreenSelection_GoesInside()
    {
        var origin = SelectionGeometry.ToolbarOrigin(new Rect(0, 0, 1000, 800), new Size(150, 40), Area);

        Assert.Equal(new Point(846, 752), origin);
    }
}
