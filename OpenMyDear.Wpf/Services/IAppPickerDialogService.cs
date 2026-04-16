using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IAppPickerDialogService
{
    InstalledAppModel? PickApp(IReadOnlyList<InstalledAppModel> apps);
}
