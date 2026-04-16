using System.Windows;
using OpenMyDear.Wpf.Services;
using OpenMyDear.Wpf.ViewModels;
using OpenMyDear.Wpf.Views;

namespace OpenMyDear.Wpf;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var storageService = new JsonProfileStorageService();
        var launcherService = new ProfileLauncherService();
        var mainWindowViewModel = new MainWindowViewModel(storageService, launcherService);
        await mainWindowViewModel.InitializeAsync();

        var mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        mainWindow.Show();
    }
}

