namespace OpenMyDear.Wpf.Services;

public interface IFolderPickerService
{
    string? PickFolder(string? initialDirectory = null);

    string? PickFile(string? initialDirectory = null);

    string? PickExecutable(string? initialDirectory = null);
}
