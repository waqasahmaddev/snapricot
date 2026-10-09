using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>
/// A frameless, topmost, <b>opaque</b> window covering exactly one display, showing that display's frozen image with
/// the custom area and its toolbar. One window per display keeps per-monitor DPI simple. Windows are reused across
/// captures and stay invisible until they have their image, final size, and position, then appear in one step.
/// Keys: Esc cancels, Enter copies (the whole display if nothing is selected), arrows nudge 1 px (Shift: 10 px).
/// </summary>
internal sealed class RegionOverlayWindow : Window
{
    private readonly Action _refreshCursor;
    private readonly RegionSelectionSurface _surface = new();
    private readonly CustomAreaToolbar _toolbar = new() { IsVisible = false };
    private readonly Image _background = new() { Stretch = Stretch.Fill, IsHitTestVisible = false };
    private FrozenDisplay? _display;
    private Action<RegionSelection?>? _done;
    private Action? _onRevealed;
    private Bitmap? _frozen;
    private double? _presetAspect;
    private bool _hasOpened;

    public RegionOverlayWindow(Action refreshCursor)
    {
        _refreshCursor = refreshCursor;

        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = false;
        Background = Brushes.Black;
        Cursor = new Cursor(StandardCursorType.Arrow);
        WindowStartupLocation = WindowStartupLocation.Manual;
        Opacity = 0;

        // The frozen screenshot is a static layer, drawn pixel-for-pixel (no smoothing, so colors and text stay exact).
        RenderOptions.SetBitmapInterpolationMode(_background, BitmapInterpolationMode.None);

        _surface.SelectionChanged += UpdateToolbar;
        _surface.Confirmed += () => Finish(CaptureIntent.Copy);
        _surface.Cancelled += () => Complete(null);

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
        _toolbar.CancelClicked += () => Complete(null);

        var layer = new Canvas();
        layer.Children.Add(_toolbar);
        Content = new Panel { Children = { _background, _surface, layer } };

        // Tunnel so arrow keys nudge the area instead of moving focus between toolbar buttons.
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>The display this window is sized for; the selector reuses it while the display is unchanged.</summary>
    public DisplayInfo? Display => _display?.Info;

    /// <summary>
    /// Shows a new frozen image and waits for the user. <paramref name="done"/> is called exactly once;
    /// <paramref name="onRevealed"/> once the window is visible.
    /// </summary>
    public void Begin(FrozenDisplay display, bool showMagnifier, Action<RegionSelection?> done, Action? onRevealed)
    {
        _display = display;
        _done = done;
        _onRevealed = onRevealed;
        _presetAspect = null;

        var previous = _frozen;
        _frozen = CapturedImageBitmap.ToDisplayBitmap(display.Image);
        _background.Source = _frozen;
        _surface.Reset(_frozen, new Core.Capture.PixelPoint(display.Info.Bounds.X, display.Info.Bounds.Y), showMagnifier);
        _toolbar.Reset();
        previous?.Dispose();

        PlaceOnDisplay(_hasOpened ? RenderScaling : display.Info.Scale);
        Opacity = 0;
        Show();
        if (_hasOpened)
        {
            Dispatcher.UIThread.Post(Reveal, DispatcherPriority.Loaded);
        }
    }

    /// <summary>Hides the window for reuse.</summary>
    public void End()
    {
        _done = null;
        _onRevealed = null;
        Opacity = 0;
        Hide();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _hasOpened = true;

        // Only now does the window know its real scale on this monitor: snap to the display's exact pixels, then reveal.
        PlaceOnDisplay(RenderScaling);
        Dispatcher.UIThread.Post(Reveal, DispatcherPriority.Loaded);
    }

    private void PlaceOnDisplay(double scale)
    {
        if (_display is not { } display)
        {
            return;
        }

        var bounds = display.Info.Bounds;
        Position = new Avalonia.PixelPoint(bounds.X, bounds.Y);
        Width = bounds.Width / scale;
        Height = bounds.Height / scale;
    }

    private void Reveal()
    {
        if (_done is null)
        {
            return; // finished or cancelled before it was revealed
        }

        Opacity = 1;
        _refreshCursor(); // the normal arrow now, not the cursor of whatever app was under the mouse
        var revealed = _onRevealed;
        _onRevealed = null;
        revealed?.Invoke();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Complete(null);
            e.Handled = true;
            return;
        }

        if (_toolbar.IsEditingSize || _display is not { } display)
        {
            return; // typing a size: Enter and arrows belong to the text box
        }

        var step = (e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1) / _surface.PixelsPerDip;
        switch (e.Key)
        {
            case Key.Enter when _surface.Selection is null:
                Complete(new RegionSelection(display, new PixelBounds(0, 0, display.Image.Width, display.Image.Height)));
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
        if (!visible || _surface.Selection is not { } selection || _display is not { } display)
        {
            return;
        }

        if (SelectionGeometry.ToPixels(selection, _surface.PixelsPerDip, display.Image.Width, display.Image.Height) is { } pixels)
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
        if (_display is { } display && _surface.Selection is { } selection &&
            SelectionGeometry.ToPixels(selection, _surface.PixelsPerDip, display.Image.Width, display.Image.Height) is { } pixels)
        {
            Complete(new RegionSelection(display, pixels, intent));
        }
    }

    private void Complete(RegionSelection? result)
    {
        var done = _done;
        _done = null;
        done?.Invoke(result);
    }
}
