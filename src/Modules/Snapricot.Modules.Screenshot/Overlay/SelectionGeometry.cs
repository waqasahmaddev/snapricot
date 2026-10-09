using Avalonia;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>Which edges a resize handle moves. Corners combine two edges.</summary>
[Flags]
public enum HandleEdges
{
    None = 0,
    Left = 1,
    Top = 2,
    Right = 4,
    Bottom = 8,
}

/// <summary>Result of hit-testing the pointer against a selection.</summary>
public readonly record struct SelectionHit(HandleEdges Edges, bool Inside);

/// <summary>
/// Pure math for the custom area: DIPs vs image pixels, hit-testing, resizing, moving, aspect ratio, and
/// toolbar placement. No UI types beyond Avalonia's Point/Rect/Size/Vector, so all of it is unit-tested.
/// </summary>
public static class SelectionGeometry
{
    private static readonly HandleEdges[] HandleOrder =
    [
        HandleEdges.Left | HandleEdges.Top, HandleEdges.Top, HandleEdges.Right | HandleEdges.Top, HandleEdges.Right,
        HandleEdges.Right | HandleEdges.Bottom, HandleEdges.Bottom, HandleEdges.Left | HandleEdges.Bottom, HandleEdges.Left,
    ];

    /// <summary>The rectangle spanned by a drag, whichever direction it went.</summary>
    public static Rect Normalize(Point anchor, Point current) =>
        new(Math.Min(anchor.X, current.X), Math.Min(anchor.Y, current.Y),
            Math.Abs(current.X - anchor.X), Math.Abs(current.Y - anchor.Y));

    /// <summary>The 8 handle centers: corners and edge midpoints, with the edges each one moves.</summary>
    public static IEnumerable<(HandleEdges Edges, Point Center)> Handles(Rect selection) =>
        HandleOrder.Select(edges => (edges, new Point(
            edges.HasFlag(HandleEdges.Left) ? selection.Left : edges.HasFlag(HandleEdges.Right) ? selection.Right : selection.Center.X,
            edges.HasFlag(HandleEdges.Top) ? selection.Top : edges.HasFlag(HandleEdges.Bottom) ? selection.Bottom : selection.Center.Y)));

    /// <summary>Handles win over the inside, so small selections stay resizable.</summary>
    public static SelectionHit HitTest(Rect selection, Point point, double handleSize)
    {
        var half = handleSize / 2;
        foreach (var (edges, center) in Handles(selection))
        {
            if (Math.Abs(point.X - center.X) <= half && Math.Abs(point.Y - center.Y) <= half)
            {
                return new SelectionHit(edges, Inside: false);
            }
        }

        return new SelectionHit(HandleEdges.None, selection.Contains(point));
    }

    /// <summary>Moves the given edges by <paramref name="delta"/>, keeping a minimum size and staying inside <paramref name="area"/>.</summary>
    public static Rect Resize(Rect original, HandleEdges edges, Vector delta, double? aspect, double minSize, Size area)
    {
        double left = original.Left, top = original.Top, right = original.Right, bottom = original.Bottom;
        if (edges.HasFlag(HandleEdges.Left))
        {
            left = Math.Clamp(left + delta.X, 0, right - minSize);
        }

        if (edges.HasFlag(HandleEdges.Right))
        {
            right = Math.Clamp(right + delta.X, left + minSize, area.Width);
        }

        if (edges.HasFlag(HandleEdges.Top))
        {
            top = Math.Clamp(top + delta.Y, 0, bottom - minSize);
        }

        if (edges.HasFlag(HandleEdges.Bottom))
        {
            bottom = Math.Clamp(bottom + delta.Y, top + minSize, area.Height);
        }

        var resized = new Rect(left, top, right - left, bottom - top);
        return aspect is > 0 ? FitAspect(resized, edges, aspect.Value, area) : resized;
    }

    /// <summary>
    /// Forces width/height = <paramref name="aspect"/>, keeping the edges opposite to <paramref name="edges"/> fixed
    /// and shrinking if needed so the result stays inside <paramref name="area"/>.
    /// </summary>
    public static Rect FitAspect(Rect rect, HandleEdges edges, double aspect, Size area)
    {
        var widthDrives = edges.HasFlag(HandleEdges.Left) || edges.HasFlag(HandleEdges.Right);
        var width = widthDrives ? rect.Width : rect.Height * aspect;
        var height = widthDrives ? rect.Width / aspect : rect.Height;

        var availableWidth = edges.HasFlag(HandleEdges.Left) ? rect.Right : area.Width - rect.Left;
        var availableHeight = edges.HasFlag(HandleEdges.Top) ? rect.Bottom : area.Height - rect.Top;
        var scale = Math.Min(1, Math.Min(availableWidth / Math.Max(width, double.Epsilon), availableHeight / Math.Max(height, double.Epsilon)));
        width *= scale;
        height *= scale;

        var x = edges.HasFlag(HandleEdges.Left) ? rect.Right - width : rect.Left;
        var y = edges.HasFlag(HandleEdges.Top) ? rect.Bottom - height : rect.Top;
        return new Rect(x, y, width, height);
    }

    /// <summary>The edges a fresh drag is "pulling", based on its direction from the anchor.</summary>
    public static HandleEdges DragEdges(Point anchor, Point current) =>
        (current.X < anchor.X ? HandleEdges.Left : HandleEdges.Right) | (current.Y < anchor.Y ? HandleEdges.Top : HandleEdges.Bottom);

    /// <summary>Moves the whole selection, clamped so it never leaves <paramref name="area"/>.</summary>
    public static Rect Move(Rect original, Vector delta, Size area) =>
        new(Math.Clamp(original.X + delta.X, 0, Math.Max(0, area.Width - original.Width)),
            Math.Clamp(original.Y + delta.Y, 0, Math.Max(0, area.Height - original.Height)),
            original.Width, original.Height);

    /// <summary>Applies an exact pixel size typed in the toolbar, keeping the top-left where possible.</summary>
    public static Rect WithPixelSize(Rect current, int widthPixels, int heightPixels, double pixelsPerDip, Size area)
    {
        var width = Math.Clamp(widthPixels / pixelsPerDip, 1 / pixelsPerDip, area.Width);
        var height = Math.Clamp(heightPixels / pixelsPerDip, 1 / pixelsPerDip, area.Height);
        return new Rect(Math.Min(current.X, area.Width - width), Math.Min(current.Y, area.Height - height), width, height);
    }

    /// <summary>
    /// Converts a DIP rectangle to image pixels, growing outward to whole pixels and clamping to the image.
    /// Returns <see langword="null"/> when nothing of the selection is left.
    /// </summary>
    public static PixelBounds? ToPixels(Rect dips, double scale, int imageWidth, int imageHeight)
    {
        var left = Math.Clamp((int)Math.Floor(dips.Left * scale), 0, imageWidth);
        var top = Math.Clamp((int)Math.Floor(dips.Top * scale), 0, imageHeight);
        var right = Math.Clamp((int)Math.Ceiling(dips.Right * scale), 0, imageWidth);
        var bottom = Math.Clamp((int)Math.Ceiling(dips.Bottom * scale), 0, imageHeight);

        var bounds = new PixelBounds(left, top, right - left, bottom - top);
        return bounds.IsEmpty ? null : bounds;
    }

    /// <summary>Where to put the magnifier: below-right of the cursor, flipped so it stays on screen.</summary>
    public static Point MagnifierOrigin(Point cursor, Size surface, double size, double offset)
    {
        var x = cursor.X + offset + size > surface.Width ? cursor.X - offset - size : cursor.X + offset;
        var y = cursor.Y + offset + size > surface.Height ? cursor.Y - offset - size : cursor.Y + offset;
        return new Point(Math.Max(0, x), Math.Max(0, y));
    }

    /// <summary>Toolbar sits below the selection, right-aligned; flips above it, or inside it, when there's no room.</summary>
    public static Point ToolbarOrigin(Rect selection, Size toolbar, Size area, double gap = 8, double margin = 4)
    {
        var x = Math.Clamp(selection.Right - toolbar.Width, margin, Math.Max(margin, area.Width - toolbar.Width - margin));

        var y = selection.Bottom + gap;
        if (y + toolbar.Height > area.Height - margin)
        {
            y = selection.Top - gap - toolbar.Height;
        }

        if (y < margin)
        {
            y = Math.Max(margin, selection.Bottom - gap - toolbar.Height);
        }

        return new Point(x, y);
    }
}
