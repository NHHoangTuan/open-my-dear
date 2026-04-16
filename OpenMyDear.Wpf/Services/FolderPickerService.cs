using System.Windows.Forms;

namespace OpenMyDear.Wpf.Services;

public sealed class FolderPickerService : IFolderPickerService
{
    public string? PickFolder(string? initialDirectory = null)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select storage directory",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        var result = dialog.ShowDialog();
        return result == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
