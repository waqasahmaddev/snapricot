using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Snapricot.Modules.Screenshot.Settings;

public partial class SaveToSettingsView : UserControl
{
    public SaveToSettingsView()
    {
        InitializeComponent();
        ChangeButton.Click += async (_, _) => await PickFolderAsync();
    }

    private async Task PickFolderAsync()
    {
        if (DataContext is not SaveToSettingsViewModel viewModel || TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
        {
            return;
        }

        var picked = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose where to save screenshots",
            AllowMultiple = false,
            SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(viewModel.Folder),
        });

        if (picked.Count > 0 && picked[0].TryGetLocalPath() is { } path)
        {
            viewModel.ChooseFolder(path);
        }
    }
}
