using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Snapricot.Core.Capture;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Feedback;

internal sealed partial class CaptureToast : Window
{
    private const int EdgeMargin = 12;
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(4);
    private readonly DispatcherTimer _timer = new() { Interval = Lifetime };
    private Bitmap? _thumbnail;
    private Action? _onClick;
    private NotificationPosition _position;

    public CaptureToast()
    {
        InitializeComponent();

        _timer.Tick += (_, _) => Close();
        CloseButton.Click += (_, e) =>
        {
            e.Handled = true;
            Close();
        };
        PointerEntered += (_, _) => _timer.Stop();  // hovering keeps it open
        PointerExited += (_, _) => _timer.Start();
        Opened += (_, _) =>
        {
            // Place only after layout has produced the final size (SizeToContent), then reveal: no jump, no mid-screen toast.
            Dispatcher.UIThread.Post(() =>
            {
                Place();
                Opacity = 1;
                _timer.Start();
            }, DispatcherPriority.Loaded);
        };
        Closed += (_, _) =>
        {
            _timer.Stop();
            _thumbnail?.Dispose();
        };
    }

    public void Show(CaptureDelivery delivery, Action? onClick, NotificationPosition position)
    {
        _thumbnail = CapturedImageBitmap.ToDisplayBitmap(delivery.Image);
        Thumbnail.Source = _thumbnail;
        _onClick = onClick;
        _position = position;

        TitleText.Text = (delivery.Copied, delivery.SavedPath is not null) switch
        {
            (true, true) => "Copied and saved",
            (true, false) => "Copied to clipboard",
            _ => "Saved",
        };
        DetailText.Text = delivery.SavedPath is { } path
            ? $"{Path.GetFileName(path)} · click to show in folder"
            : $"{delivery.Image.Width} × {delivery.Image.Height} px";
        ToolTip.SetTip(DetailText, delivery.SavedPath);

        Opacity = 0;
        Show();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _onClick?.Invoke();
        Close();
    }

    private void Place()
    {
        if (Screens.Primary is not { } screen)
        {
            return;
        }

        var area = screen.WorkingArea;
        var origin = ToastPlacement.Origin(
            new PixelBounds(area.X, area.Y, area.Width, area.Height),
            (int)Math.Ceiling(Bounds.Width * screen.Scaling),
            (int)Math.Ceiling(Bounds.Height * screen.Scaling),
            _position,
            (int)Math.Round(EdgeMargin * screen.Scaling));
        Position = new Avalonia.PixelPoint(origin.X, origin.Y);
    }
}
