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

    public string? PickFile(string? initialDirectory = null)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            Title = "Select file"
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    public string? PickExecutable(string? initialDirectory = null)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Applications (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            Title = "Select application"
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }
}
