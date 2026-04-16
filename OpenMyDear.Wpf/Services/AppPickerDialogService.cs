using OpenMyDear.Wpf.Models;
using OpenMyDear.Wpf.ViewModels;
using OpenMyDear.Wpf.Views;

namespace OpenMyDear.Wpf.Services;

public sealed class AppPickerDialogService : IAppPickerDialogService
{
    private readonly ILocalizationService _localizationService;

    public AppPickerDialogService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public InstalledAppModel? PickApp(IReadOnlyList<InstalledAppModel> apps)
    {
        var viewModel = new InstalledAppsPickerViewModel(apps, _localizationService);
        var window = new InstalledAppsPickerWindow
        {
            DataContext = viewModel
        };

        var result = window.ShowDialog();
        return result == true ? viewModel.SelectedApp : null;
    }
}
