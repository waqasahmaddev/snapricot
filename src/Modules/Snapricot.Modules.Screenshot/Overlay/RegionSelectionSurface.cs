using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using CorePixelPoint = Snapricot.Core.Capture.PixelPoint;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>
/// The custom area layer: dims outside the selection and lets the user draw, move, and resize it with 8 handles.
/// It does <b>not</b> draw the frozen screenshot: that sits underneath in a separate, static <c>Image</c> the compositor
/// caches on the GPU, so a pointer move only repaints this light layer (a few rectangles and a label).
/// The frozen bitmap is only read by the optional magnifier. Instances are reused across captures (<see cref="Reset"/>).
/// </summary>
internal sealed class RegionSelectionSurface : Control
{
    private Bitmap? _frozen;
    private CorePixelPoint _displayOrigin;
    private bool _showMagnifier;

    private const double HandleSize = 10;
    private const double HandleHitSize = 16;
    private const double MinSize = 4;
    private const double MagnifierSize = 120;
    private const double MagnifierOffset = 24;
    private const int MagnifierPixels = 15; // odd, so there's a center pixel

    // Brand colors: Apricot deep accent, Plum label background.
    private static readonly IImmutableSolidColorBrush AccentBrush = new ImmutableSolidColorBrush(Color.FromRgb(0xD9, 0x62, 0x2B));
    // Light dim, and only outside a selection: before you draw, the screen looks exactly as it did (like Icecream).
    private static readonly IImmutableSolidColorBrush DimBrush = new ImmutableSolidColorBrush(Color.FromArgb(0x4D, 0, 0, 0));
    private static readonly IImmutableSolidColorBrush PlumBrush = new ImmutableSolidColorBrush(Color.FromArgb(0xE6, 0x2E, 0x22, 0x40));
    private static readonly ImmutablePen SelectionPen = new(AccentBrush, 1.5, new ImmutableDashStyle([4, 3], 0));
    private static readonly ImmutablePen HandlePen = new(AccentBrush, 1.5);
    private static readonly ImmutablePen MagnifierPen = new(Brushes.White, 2);

    private readonly Dictionary<HandleEdges, Cursor> _cursors = [];
    private Mode _mode;
    private Point _start;
    private Rect _original;
    private HandleEdges _edges;
    private Point _pointer = new(-1, -1);
    private string? _labelText;
    private FormattedText? _label;

    private enum Mode
    {
        None,
        Drawing,
        Moving,
        Resizing,
    }

    /// <summary>Raised whenever the selection changes or an interaction starts/ends.</summary>
    public event Action? SelectionChanged;

    /// <summary>Double-click inside the selection.</summary>
    public event Action? Confirmed;

    /// <summary>Right-click with no selection.</summary>
    public event Action? Cancelled;

    public Rect? Selection { get; private set; }

    /// <summary>True while the user is dragging (drawing, moving, or resizing).</summary>
    public bool IsInteracting => _mode != Mode.None;

    /// <summary>Width/height ratio to enforce, or <see langword="null"/> for free.</summary>
    public double? AspectRatio { get; set; }

    /// <summary>Exact mapping from this control's DIPs to frozen-image pixels (robust to any DPI rounding).</summary>
    public double PixelsPerDip => Bounds.Width > 0 && _frozen is { } frozen ? frozen.PixelSize.Width / Bounds.Width : 1;

    /// <summary>Starts a fresh capture: new frozen image, no selection, no aspect lock.</summary>
    public void Reset(Bitmap frozen, CorePixelPoint displayOrigin, bool showMagnifier)
    {
        (_frozen, _displayOrigin, _showMagnifier) = (frozen, displayOrigin, showMagnifier);
        (_mode, Selection, AspectRatio, _labelText, _label) = (Mode.None, null, null, null, null);
        _pointer = new Point(-1, -1);
        Cursor = null; // the normal arrow; handles still show resize/move cursors on hover
        InvalidateVisual();
    }

    public void SetSelection(Rect? selection)
    {
        Selection = selection;
        SelectionChanged?.Invoke();
        InvalidateVisual();
    }

    /// <summary>Arrow-key nudge, in DIPs.</summary>
    public void Nudge(Vector delta)
    {
        if (Selection is { } selection)
        {
            SetSelection(SelectionGeometry.Move(selection, delta, Bounds.Size));
        }
    }

    /// <summary>Applies the current <see cref="AspectRatio"/> to the existing selection, keeping its top-left.</summary>
    public void ApplyAspect()
    {
        if (Selection is { } selection && AspectRatio is > 0)
        {
            SetSelection(SelectionGeometry.FitAspect(selection, HandleEdges.Right | HandleEdges.Bottom, AspectRatio.Value, Bounds.Size));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var position = point.Position;
        e.Handled = true;

        if (point.Properties.IsRightButtonPressed)
        {
            if (Selection is null)
            {
                Cancelled?.Invoke();
            }
            else
            {
                SetSelection(null); // right-click clears the area so you can start over
            }

            return;
        }

        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var hit = Selection is { } current ? SelectionGeometry.HitTest(current, position, HandleHitSize) : default;
        if (hit.Inside && e.ClickCount == 2)
        {
            Confirmed?.Invoke();
            return;
        }

        _start = position;
        if (hit.Edges != HandleEdges.None)
        {
            (_mode, _edges, _original) = (Mode.Resizing, hit.Edges, Selection!.Value);
        }
        else if (hit.Inside)
        {
            (_mode, _original) = (Mode.Moving, Selection!.Value);
        }
        else
        {
            _mode = Mode.Drawing;
            Selection = new Rect(position, default(Size));
        }

        e.Pointer.Capture(this);
        SelectionChanged?.Invoke();
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var area = Bounds.Size;
        var position = e.GetPosition(this);
        _pointer = new Point(Math.Clamp(position.X, 0, area.Width), Math.Clamp(position.Y, 0, area.Height));

        switch (_mode)
        {
            case Mode.Drawing:
                var drawn = SelectionGeometry.Normalize(_start, _pointer);
                Selection = AspectRatio is > 0
                    ? SelectionGeometry.FitAspect(drawn, SelectionGeometry.DragEdges(_start, _pointer), AspectRatio.Value, area)
                    : drawn;
                SelectionChanged?.Invoke();
                break;
            case Mode.Moving:
                Selection = SelectionGeometry.Move(_original, _pointer - _start, area);
                SelectionChanged?.Invoke();
                break;
            case Mode.Resizing:
                Selection = SelectionGeometry.Resize(_original, _edges, _pointer - _start, AspectRatio, MinSize, area);
                SelectionChanged?.Invoke();
                break;
            default:
                Cursor = CursorFor(Selection is { } s ? SelectionGeometry.HitTest(s, position, HandleHitSize) : default);
                break;
        }

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_mode == Mode.None || e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        // A click without a meaningful drag isn't a selection (window picking arrives later).
        if (_mode == Mode.Drawing && Selection is { } drawn && (drawn.Width * PixelsPerDip < 2 || drawn.Height * PixelsPerDip < 2))
        {
            Selection = null;
        }

        _mode = Mode.None;
        e.Pointer.Capture(null);
        SelectionChanged?.Invoke();
        InvalidateVisual();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        if (_mode == Mode.None)
        {
            _pointer = new Point(-1, -1);
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        var full = new Rect(Bounds.Size);
        context.FillRectangle(Brushes.Transparent, full); // keeps the whole layer hit-testable

        if (Selection is { } selection)
        {
            context.FillRectangle(DimBrush, new Rect(0, 0, full.Width, selection.Top));
            context.FillRectangle(DimBrush, new Rect(0, selection.Bottom, full.Width, full.Height - selection.Bottom));
            context.FillRectangle(DimBrush, new Rect(0, selection.Top, selection.Left, selection.Height));
            context.FillRectangle(DimBrush, new Rect(selection.Right, selection.Top, full.Width - selection.Right, selection.Height));
            context.DrawRectangle(null, SelectionPen, selection);
            if (_mode != Mode.Drawing)
            {
                DrawHandles(context, selection);
            }

            DrawSizeLabel(context, selection, full);
        }

        // Optional (Settings → "Area selection magnifier", off by default). Hidden while just looking at or moving the area.
        var wantsMagnifier = _showMagnifier && _frozen is not null &&
            (_mode is Mode.Drawing or Mode.Resizing || (_mode == Mode.None && Selection is null));
        if (wantsMagnifier && full.Contains(_pointer))
        {
            DrawMagnifier(context, full);
        }
    }

    private static void DrawHandles(DrawingContext context, Rect selection)
    {
        foreach (var (_, center) in SelectionGeometry.Handles(selection))
        {
            var handle = new Rect(center.X - (HandleSize / 2), center.Y - (HandleSize / 2), HandleSize, HandleSize);
            context.FillRectangle(Brushes.White, handle);
            context.DrawRectangle(null, HandlePen, handle);
        }
    }

    private void DrawSizeLabel(DrawingContext context, Rect selection, Rect full)
    {
        // Reuse the FormattedText while the size is unchanged (moving the area doesn't re-shape text).
        var value = $"{Math.Round(selection.Width * PixelsPerDip)} × {Math.Round(selection.Height * PixelsPerDip)}";
        if (value != _labelText || _label is null)
        {
            (_labelText, _label) = (value, Text(value, 12));
        }

        var text = _label;
        var box = new Size(text.Width + 12, text.Height + 6);
        var y = selection.Top - box.Height - 8 >= 0 ? selection.Top - box.Height - 8 : selection.Top + 8;
        var x = Math.Clamp(selection.Left, 0, Math.Max(0, full.Width - box.Width));

        context.FillRectangle(AccentBrush, new Rect(new Point(x, y), box));
        context.DrawText(text, new Point(x + 6, y + 3));
    }

    private void DrawMagnifier(DrawingContext context, Rect full)
    {
        var origin = SelectionGeometry.MagnifierOrigin(_pointer, full.Size, MagnifierSize, MagnifierOffset);
        var lens = new Rect(origin, new Size(MagnifierSize, MagnifierSize));

        // Source area in bitmap units (1 unit = 1 physical pixel), centered on the pixel under the cursor.
        var pixelX = Math.Floor(_pointer.X * PixelsPerDip);
        var pixelY = Math.Floor(_pointer.Y * PixelsPerDip);
        var half = MagnifierPixels / 2;
        var source = new Rect(pixelX - half, pixelY - half, MagnifierPixels, MagnifierPixels);

        using (context.PushGeometryClip(new EllipseGeometry(lens)))
        using (context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None }))
        {
            context.FillRectangle(Brushes.Black, lens);
            context.DrawImage(_frozen!, source, lens);

            var cell = MagnifierSize / MagnifierPixels;
            context.DrawRectangle(null, HandlePen, new Rect(lens.X + (half * cell), lens.Y + (half * cell), cell, cell));
        }

        context.DrawEllipse(null, MagnifierPen, lens);

        var coordinates = Text(
            string.Create(CultureInfo.InvariantCulture, $"{_displayOrigin.X + pixelX}, {_displayOrigin.Y + pixelY}"), 11);
        var labelBox = new Rect(lens.X + ((MagnifierSize - coordinates.Width - 10) / 2), lens.Bottom + 6, coordinates.Width + 10, coordinates.Height + 4);
        context.FillRectangle(PlumBrush, labelBox);
        context.DrawText(coordinates, new Point(labelBox.X + 5, labelBox.Y + 2));
    }

    private Cursor CursorFor(SelectionHit hit)
    {
        var key = hit.Inside ? (HandleEdges)(-1) : hit.Edges;
        if (!_cursors.TryGetValue(key, out var cursor))
        {
            cursor = new Cursor(hit.Inside ? StandardCursorType.SizeAll : hit.Edges switch
            {
                HandleEdges.Left | HandleEdges.Top => StandardCursorType.TopLeftCorner,
                HandleEdges.Right | HandleEdges.Top => StandardCursorType.TopRightCorner,
                HandleEdges.Left | HandleEdges.Bottom => StandardCursorType.BottomLeftCorner,
                HandleEdges.Right | HandleEdges.Bottom => StandardCursorType.BottomRightCorner,
                HandleEdges.Left => StandardCursorType.LeftSide,
                HandleEdges.Right => StandardCursorType.RightSide,
                HandleEdges.Top => StandardCursorType.TopSide,
                HandleEdges.Bottom => StandardCursorType.BottomSide,
                _ => StandardCursorType.Arrow,
            });
            _cursors[key] = cursor;
        }

        return cursor;
    }

    private static FormattedText Text(string value, double size) =>
        new(value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, size, Brushes.White);
}
