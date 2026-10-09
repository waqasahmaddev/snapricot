using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Feedback;

internal sealed partial class CaptureToast : Window
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(4);
    private readonly DispatcherTimer _timer = new() { Interval = Lifetime };
    private Bitmap? _thumbnail;
    private Action? _onClick;

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
            PlaceBottomRight();
            _timer.Start();
        };
        Closed += (_, _) =>
        {
            _timer.Stop();
            _thumbnail?.Dispose();
        };
    }

    public void Show(CaptureDelivery delivery, Action? onClick)
    {
        _thumbnail = CapturedImageBitmap.ToBitmap(delivery.Image);
        Thumbnail.Source = _thumbnail;
        _onClick = onClick;

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

        Show();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _onClick?.Invoke();
        Close();
    }

    private void PlaceBottomRight()
    {
        if ((Screens.ScreenFromWindow(this) ?? Screens.Primary) is not { } screen)
        {
            return;
        }

        const int margin = 12;
        var area = screen.WorkingArea;
        var width = (int)Math.Ceiling(Bounds.Width * screen.Scaling);
        var height = (int)Math.Ceiling(Bounds.Height * screen.Scaling);
        Position = new PixelPoint(area.Right - width - margin, area.Bottom - height - margin);
    }
}
