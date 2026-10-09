using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>
/// A frameless, topmost window covering exactly one display, showing that display's frozen image with the
/// custom area and its toolbar. One window per display keeps per-monitor DPI simple: each has a single scale factor.
/// Keys: Esc cancels, Enter copies (the whole display if nothing is selected), arrows nudge 1 px (Shift: 10 px).
/// </summary>
internal sealed class RegionOverlayWindow : Window
{
    private readonly FrozenDisplay _display;
    private readonly Action<RegionSelection?> _done;
    private readonly RegionSelectionSurface _surface;
    private readonly CustomAreaToolbar _toolbar = new() { IsVisible = false };
    private double? _presetAspect;

    public RegionOverlayWindow(FrozenDisplay display, bool showMagnifier, Action<RegionSelection?> done)
    {
        _display = display;
        _done = done;
        var bounds = display.Info.Bounds;

        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = false;
        Background = Brushes.Black;
        Cursor = new Cursor(StandardCursorType.Cross);
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = new Avalonia.PixelPoint(bounds.X, bounds.Y);
        Width = bounds.Width / display.Info.Scale;
        Height = bounds.Height / display.Info.Scale;

        _surface = new RegionSelectionSurface(
            CapturedImageBitmap.ToBitmap(display.Image), new Core.Capture.PixelPoint(bounds.X, bounds.Y), showMagnifier);
        _surface.SelectionChanged += UpdateToolbar;
        _surface.Confirmed += () => Finish(CaptureIntent.Copy);
        _surface.Cancelled += () => _done(null);

        _toolbar.AspectChanged += ratio =>
        {
            _presetAspect = ratio;
            _surface.AspectRatio = ratio;
            _surface.ApplyAspect();
        };
        _toolbar.LockChanged += locked =>
            _surface.AspectRatio = locked && _surface.Selection is { Height: > 0 } s ? s.Width / s.Height : _presetAspect;
        _toolbar.SizeEntered += (width, height) =>
        {
            if (_surface.Selection is { } selection)
            {
                _surface.SetSelection(SelectionGeometry.WithPixelSize(selection, width, height, _surface.PixelsPerDip, _surface.Bounds.Size));
            }
        };
        _toolbar.CopyClicked += () => Finish(CaptureIntent.Copy);
        _toolbar.SaveClicked += () => Finish(CaptureIntent.Save);
        _toolbar.CancelClicked += () => _done(null);

        var layer = new Canvas();
        layer.Children.Add(_toolbar);
        Content = new Panel { Children = { _surface, layer } };

        // Tunnel so arrow keys nudge the area instead of moving focus between toolbar buttons.
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // The window only knows its real scale once it's on its monitor; snap to exactly the display's pixels.
        var bounds = _display.Info.Bounds;
        Position = new Avalonia.PixelPoint(bounds.X, bounds.Y);
        Width = bounds.Width / RenderScaling;
        Height = bounds.Height / RenderScaling;
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _done(null);
            e.Handled = true;
            return;
        }

        if (_toolbar.IsEditingSize)
        {
            return; // typing a size: Enter and arrows belong to the text box
        }

        var step = (e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1) / _surface.PixelsPerDip;
        switch (e.Key)
        {
            case Key.Enter when _surface.Selection is null:
                var image = _display.Image;
                _done(new RegionSelection(_display, new PixelBounds(0, 0, image.Width, image.Height)));
                break;
            case Key.Enter:
                Finish(CaptureIntent.Copy);
                break;
            case Key.Left:
                _surface.Nudge(new Vector(-step, 0));
                break;
            case Key.Right:
                _surface.Nudge(new Vector(step, 0));
                break;
            case Key.Up:
                _surface.Nudge(new Vector(0, -step));
                break;
            case Key.Down:
                _surface.Nudge(new Vector(0, step));
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void UpdateToolbar()
    {
        var visible = _surface.Selection is not null && !_surface.IsInteracting;
        _toolbar.IsVisible = visible;
        if (!visible || _surface.Selection is not { } selection)
        {
            return;
        }

        if (SelectionGeometry.ToPixels(selection, _surface.PixelsPerDip, _display.Image.Width, _display.Image.Height) is { } pixels)
        {
            _toolbar.ShowSize(pixels.Width, pixels.Height);
        }

        _toolbar.Measure(Size.Infinity);
        var origin = SelectionGeometry.ToolbarOrigin(selection, _toolbar.DesiredSize, _surface.Bounds.Size);
        Canvas.SetLeft(_toolbar, origin.X);
        Canvas.SetTop(_toolbar, origin.Y);
    }

    private void Finish(CaptureIntent intent)
    {
        if (_surface.Selection is { } selection &&
            SelectionGeometry.ToPixels(selection, _surface.PixelsPerDip, _display.Image.Width, _display.Image.Height) is { } pixels)
        {
            _done(new RegionSelection(_display, pixels, intent));
        }
    }
}
