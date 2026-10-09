using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Snapricot.Core.Capture;

namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>The Save dialog: file name, folder (with "Always save here"), and a dismissable tip.</summary>
internal sealed partial class SaveCaptureDialog : Window
{
    private ScreenshotPreferences? _locations;
    private Bitmap? _preview;
    private string _folder = string.Empty;

    public SaveCaptureDialog()
    {
        InitializeComponent();

        NameBox.TextChanged += (_, _) => ErrorText.IsVisible = false;
        ChangeButton.Click += async (_, _) => await PickFolderAsync();
        DismissTipButton.Click += (_, _) =>
        {
            TipPanel.IsVisible = false;
            _locations?.DismissSaveTip();
        };
        CancelButton.Click += (_, _) => Close();
        SaveButton.Click += (_, _) => TrySave();
        Opened += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
        Closed += (_, _) => _preview?.Dispose();
    }

    /// <summary>The confirmed request, or <see langword="null"/> if the dialog was closed without saving.</summary>
    public SaveRequest? Result { get; private set; }

    public void Initialize(CapturedImage image, string suggestedFileName, ScreenshotPreferences locations)
    {
        _locations = locations;
        _preview = CapturedImageBitmap.ToDisplayBitmap(image);
        Preview.Source = _preview;
        SizeText.Text = $"{image.Width} × {image.Height} px";
        var format = locations.Settings.Format;
        FormatText.Text = format.DisplayName();
        ExtensionText.Text = format.Extension();
        NameBox.Text = suggestedFileName;
        TipPanel.IsVisible = !locations.Settings.SaveTipDismissed;

        if (locations.DefaultIsMissing)
        {
            FolderNote.Text = $"Your default folder isn't available right now, so this suggests {locations.FallbackFolder}.";
            FolderNote.IsVisible = true;
        }

        SetFolder(locations.SuggestedFolder);
    }

    private void SetFolder(string folder)
    {
        _folder = folder;
        FolderText.Text = folder;
        ToolTip.SetTip(FolderText, folder);
        var isDefault = _locations?.IsDefault(folder) == true;
        DefaultBadge.IsVisible = isDefault;
        AlwaysBox.IsVisible = !isDefault;
        AlwaysBox.IsChecked = false;
    }

    private async Task PickFolderAsync()
    {
        var start = await StorageProvider.TryGetFolderFromPathAsync(_folder);
        var picked = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose where to save",
            AllowMultiple = false,
            SuggestedStartLocation = start,
        });

        if (picked.Count > 0 && picked[0].TryGetLocalPath() is { } path)
        {
            SetFolder(path);
        }
    }

    private void TrySave()
    {
        if (FileNameRules.Validate(NameBox.Text) is { } error)
        {
            ErrorText.Text = error;
            ErrorText.IsVisible = true;
            NameBox.Focus();
            return;
        }

        Result = new SaveRequest(_folder, FileNameRules.Normalize(NameBox.Text), AlwaysBox.IsChecked == true);
        Close();
    }
}
