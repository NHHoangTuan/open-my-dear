using System.Windows;
using OpenMyDear.Wpf.Services;
using OpenMyDear.Wpf.ViewModels;
using OpenMyDear.Wpf.Views;

namespace OpenMyDear.Wpf;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var localizationService = new JsonLocalizationService();
        var themeService = new ThemeService();
        var storageService = new JsonProfileStorageService();
        var configService = new JsonConfigService();
        var migrationService = new ProfileMigrationService();
        var launcherService = new ProfileLauncherService();
        var autostartService = new AutostartService();
        var installedAppDiscoveryService = new InstalledAppDiscoveryService();
        var appPickerDialogService = new AppPickerDialogService(localizationService);
        var folderPickerService = new FolderPickerService();
        var appVersionService = new AppVersionService();

        var mainWindowViewModel = new MainWindowViewModel(
            storageService,
            configService,
            migrationService,
            launcherService,
            autostartService,
            installedAppDiscoveryService,
            appPickerDialogService,
            folderPickerService,
            localizationService,
            themeService,
            appVersionService);

        await mainWindowViewModel.InitializeAsync();

        var mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        mainWindow.Show();
    }
}

