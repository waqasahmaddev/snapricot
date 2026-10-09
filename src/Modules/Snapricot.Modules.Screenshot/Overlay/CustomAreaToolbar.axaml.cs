using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;

namespace Snapricot.Modules.Screenshot.Overlay;

/// <summary>The floating toolbar next to the custom area. Raises intents; the overlay window decides what they do.</summary>
internal sealed partial class CustomAreaToolbar : UserControl
{
    private static readonly double?[] AspectRatios = [null, 16d / 9, 4d / 3, 1d];
    private bool _updating;

    public CustomAreaToolbar()
    {
        InitializeComponent();

        AspectBox.SelectionChanged += (_, _) =>
        {
            if (_updating)
            {
                return;
            }

            _updating = true;
            LockButton.IsChecked = false;
            _updating = false;
            AspectChanged?.Invoke(AspectRatios[Math.Max(0, AspectBox.SelectedIndex)]);
        };
        LockButton.IsCheckedChanged += (_, _) =>
        {
            if (!_updating)
            {
                LockChanged?.Invoke(LockButton.IsChecked == true);
            }
        };

        foreach (var box in new[] { WidthBox, HeightBox })
        {
            box.KeyDown += OnSizeKeyDown;
            box.LostFocus += (_, _) => CommitSize();
        }

        SaveButton.Click += (_, _) => SaveClicked?.Invoke();
        CopyButton.Click += (_, _) => CopyClicked?.Invoke();
        CancelButton.Click += (_, _) => CancelClicked?.Invoke();
    }

    public event Action<double?>? AspectChanged;

    public event Action<bool>? LockChanged;

    public event Action<int, int>? SizeEntered;

    public event Action? SaveClicked;

    public event Action? CopyClicked;

    public event Action? CancelClicked;

    public bool IsEditingSize => WidthBox.IsFocused || HeightBox.IsFocused;

    /// <summary>Shows the current size, unless the user is typing in one of the boxes.</summary>
    public void ShowSize(int width, int height)
    {
        if (!IsEditingSize)
        {
            WidthBox.Text = width.ToString(CultureInfo.InvariantCulture);
            HeightBox.Text = height.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void OnSizeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitSize();
            e.Handled = true; // Enter in a size box applies the size; it doesn't copy
        }
    }

    private void CommitSize()
    {
        if (int.TryParse(WidthBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
            int.TryParse(HeightBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) &&
            width > 0 && height > 0)
        {
            SizeEntered?.Invoke(width, height);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true; // clicks on the toolbar never start a new selection underneath
    }
}
