using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenMyDear.Wpf.Helpers;
using OpenMyDear.Wpf.Models;
using OpenMyDear.Wpf.Services;

namespace OpenMyDear.Wpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProfileStorageService _profileStorageService;
    private readonly IConfigService _configService;
    private readonly IProfileMigrationService _profileMigrationService;
    private readonly IProfileLauncherService _launcherService;
    private readonly IAutostartService _autostartService;
    private readonly IInstalledAppDiscoveryService _installedAppDiscoveryService;
    private readonly IAppPickerDialogService _appPickerDialogService;
    private readonly IFolderPickerService _folderPickerService;
    private readonly SemaphoreSlim _saveProfilesSemaphore = new(1, 1);
    private readonly SemaphoreSlim _saveConfigSemaphore = new(1, 1);

    private AppConfigModel _config = new();
    private bool _isInitialized;
    private bool _suspendAutoSave;

    [ObservableProperty]
    private ProfileViewModel? _selectedProfile;

    [ObservableProperty]
    private LaunchItemViewModel? _selectedItem;

    [ObservableProperty]
    private string _runStatus = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _alwaysOnTop;

    [ObservableProperty]
    private bool _autostartEnabled;

    [ObservableProperty]
    private string? _storageDirectory;

    [ObservableProperty]
    private string _selectedLanguage = "en";

    public ObservableCollection<ProfileViewModel> Profiles { get; } = [];

    public ObservableCollection<string> RunErrors { get; } = [];

    public LaunchMode[] LaunchModes { get; } = Enum.GetValues<LaunchMode>();

    public ItemType[] ItemTypes { get; } = Enum.GetValues<ItemType>();

    public ILocalizationService Localizer { get; }

    public string[] SupportedLanguages => Localizer.SupportedLanguages;

    public bool HasSelectedProfile => SelectedProfile is not null;

    public bool HasSelectedItem => SelectedItem is not null;

    public MainWindowViewModel(
        IProfileStorageService profileStorageService,
        IConfigService configService,
        IProfileMigrationService profileMigrationService,
        IProfileLauncherService launcherService,
        IAutostartService autostartService,
        IInstalledAppDiscoveryService installedAppDiscoveryService,
        IAppPickerDialogService appPickerDialogService,
        IFolderPickerService folderPickerService,
        ILocalizationService localizationService)
    {
        _profileStorageService = profileStorageService;
        _configService = configService;
        _profileMigrationService = profileMigrationService;
        _launcherService = launcherService;
        _autostartService = autostartService;
        _installedAppDiscoveryService = installedAppDiscoveryService;
        _appPickerDialogService = appPickerDialogService;
        _folderPickerService = folderPickerService;
        Localizer = localizationService;

        Profiles.CollectionChanged += OnProfilesCollectionChanged;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _config = await _configService.LoadAsync();
            StorageDirectory = _config.StorageDirectory;
            AlwaysOnTop = _config.AlwaysOnTop;
            AutostartEnabled = _config.AutostartEnabled;
            SelectedLanguage = string.IsNullOrWhiteSpace(_config.Language) ? "en" : _config.Language;

            await Localizer.SetLanguageAsync(SelectedLanguage);
            SelectedLanguage = Localizer.CurrentLanguage;

            await LoadProfilesAsync();

            if (AutostartEnabled)
            {
                var syncResult = await _autostartService.SetEnabledAsync(true);
                if (!syncResult.Succeeded)
                {
                    RunStatus = $"{Localizer["StatusAutostartSyncFailed"]}: {syncResult.Error}";
                }
            }

            _isInitialized = true;
            RunStatus = Localizer["StatusProfilesLoaded"];
        }
        catch (Exception ex)
        {
            RunStatus = $"{Localizer["StatusLoadFailed"]}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddProfile()
    {
        var nextNumber = Profiles.Count + 1;
        var profile = new ProfileViewModel
        {
            Name = $"{Localizer["ProfileDefaultName"]} {nextNumber}"
        };

        Profiles.Add(profile);
        SelectedProfile = profile;
        TriggerAutoSave();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProfile))]
    private void RemoveProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        Profiles.Remove(SelectedProfile);
        SelectedProfile = Profiles.FirstOrDefault();
        SelectedItem = SelectedProfile?.Items.FirstOrDefault();
        TriggerAutoSave();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProfile))]
    private void AddItem()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var newItem = new LaunchItemViewModel
        {
            Label = Localizer["ItemDefaultLabel"],
            Type = ItemType.App,
            Path = string.Empty
        };

        SelectedProfile.Items.Add(newItem);
        SelectedItem = newItem;
        TriggerAutoSave();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveItem))]
    private void RemoveItem()
    {
        if (SelectedProfile is null || SelectedItem is null)
        {
            return;
        }

        SelectedProfile.Items.Remove(SelectedItem);
        SelectedItem = SelectedProfile.Items.FirstOrDefault();
        TriggerAutoSave();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private async Task PickOpenWithAsync()
    {
        if (SelectedItem is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            RunStatus = Localizer["StatusLoadingApps"];

            var apps = await _installedAppDiscoveryService.GetInstalledAppsAsync();
            var selected = _appPickerDialogService.PickApp(apps);
            if (selected is null)
            {
                RunStatus = Localizer["StatusReady"];
                return;
            }

            SelectedItem.OpenWith = selected.ExecutablePath;
            SelectedItem.OpenWithName = selected.Name;
            TriggerAutoSave();
            RunStatus = Localizer["StatusOpenWithUpdated"];
        }
        catch (Exception ex)
        {
            RunStatus = $"{Localizer["StatusLoadingAppsFailed"]}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void ClearOpenWith()
    {
        if (SelectedItem is null)
        {
            return;
        }

        SelectedItem.OpenWith = null;
        SelectedItem.OpenWithName = null;
        SelectedItem.OpenWithIcon = null;
        TriggerAutoSave();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProfile))]
    private async Task RunProfileAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            RunErrors.Clear();
            RunStatus = Localizer["StatusRunningProfile"];

            var result = await _launcherService.RunAsync(SelectedProfile.ToModel());
            foreach (var warning in result.Warnings)
            {
                RunErrors.Add($"[{Localizer["RunWarning"]}] {warning}");
            }

            foreach (var error in result.Errors)
            {
                RunErrors.Add($"[{Localizer["RunError"]}] {error}");
            }

            RunStatus = string.Format(
                Localizer["StatusRunCompleted"],
                result.Total,
                result.Succeeded,
                result.Failed);
        }
        catch (Exception ex)
        {
            RunStatus = $"{Localizer["StatusRunFailed"]}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SaveNow()
    {
        TriggerAutoSave();
    }

    [RelayCommand]
    private async Task BrowseStorageDirectoryAsync()
    {
        var current = string.IsNullOrWhiteSpace(StorageDirectory)
            ? AppPaths.DefaultAppDataDirectory
            : StorageDirectory;

        var selected = _folderPickerService.PickFolder(current);
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        await ChangeStorageDirectoryAsync(selected);
    }

    [RelayCommand]
    private async Task ClearStorageDirectoryAsync()
    {
        await ChangeStorageDirectoryAsync(null);
    }

    private async Task ChangeStorageDirectoryAsync(string? newDirectory)
    {
        try
        {
            IsBusy = true;
            var migration = await _profileMigrationService.MoveProfilesAsync(_config.StorageDirectory, newDirectory);
            if (!migration.Succeeded)
            {
                RunStatus = $"{Localizer["StatusStorageUpdateFailed"]}: {migration.Error}";
                return;
            }

            _config.StorageDirectory = newDirectory;
            StorageDirectory = newDirectory;
            await SaveConfigAsync();
            await LoadProfilesAsync();
            RunStatus = Localizer["StatusStorageUpdated"];
        }
        catch (Exception ex)
        {
            RunStatus = $"{Localizer["StatusStorageUpdateFailed"]}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadProfilesAsync()
    {
        _suspendAutoSave = true;
        try
        {
            Profiles.Clear();
            var profileModels = await _profileStorageService.LoadAsync(_config.StorageDirectory);
            foreach (var model in profileModels)
            {
                Profiles.Add(new ProfileViewModel(model));
            }

            SelectedProfile = Profiles.FirstOrDefault();
            SelectedItem = SelectedProfile?.Items.FirstOrDefault();
        }
        finally
        {
            _suspendAutoSave = false;
        }
    }

    private bool CanRemoveItem()
    {
        return SelectedProfile is not null && SelectedItem is not null;
    }

    private void OnProfilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var profile in e.NewItems.OfType<ProfileViewModel>())
            {
                profile.Changed += OnProfileChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var profile in e.OldItems.OfType<ProfileViewModel>())
            {
                profile.Changed -= OnProfileChanged;
            }
        }
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        TriggerAutoSave();
    }

    private void TriggerAutoSave()
    {
        if (!_isInitialized || _suspendAutoSave)
        {
            return;
        }

        SaveProfilesAsync().SafeFireAndForget(ex =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RunStatus = $"{Localizer["StatusAutosaveFailed"]}: {ex.Message}";
            });
        });
    }

    private async Task SaveProfilesAsync(CancellationToken cancellationToken = default)
    {
        await _saveProfilesSemaphore.WaitAsync(cancellationToken);
        try
        {
            var profileModels = Profiles.Select(profile => profile.ToModel()).ToList();
            await _profileStorageService.SaveAsync(profileModels, _config.StorageDirectory, cancellationToken);
        }
        finally
        {
            _saveProfilesSemaphore.Release();
        }
    }

    private async Task SaveConfigAsync(CancellationToken cancellationToken = default)
    {
        await _saveConfigSemaphore.WaitAsync(cancellationToken);
        try
        {
            await _configService.SaveAsync(_config, cancellationToken);
        }
        finally
        {
            _saveConfigSemaphore.Release();
        }
    }

    partial void OnSelectedProfileChanged(ProfileViewModel? value)
    {
        OnPropertyChanged(nameof(HasSelectedProfile));
        SelectedItem = value?.Items.FirstOrDefault();
        RemoveProfileCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
        RunProfileCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(LaunchItemViewModel? value)
    {
        OnPropertyChanged(nameof(HasSelectedItem));
        RemoveItemCommand.NotifyCanExecuteChanged();
        PickOpenWithCommand.NotifyCanExecuteChanged();
        ClearOpenWithCommand.NotifyCanExecuteChanged();
    }

    partial void OnAlwaysOnTopChanged(bool value)
    {
        if (!_isInitialized)
        {
            return;
        }

        _config.AlwaysOnTop = value;
        SaveConfigAsync().SafeFireAndForget(ex =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RunStatus = $"{Localizer["StatusConfigSaveFailed"]}: {ex.Message}";
            });
        });
    }

    partial void OnAutostartEnabledChanged(bool value)
    {
        if (!_isInitialized)
        {
            return;
        }

        UpdateAutostartAsync(value).SafeFireAndForget(ex =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RunStatus = $"{Localizer["StatusAutostartUpdateFailed"]}: {ex.Message}";
            });
        });
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        if (!_isInitialized)
        {
            return;
        }

        UpdateLanguageAsync(value).SafeFireAndForget(ex =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RunStatus = $"{Localizer["StatusLanguageUpdateFailed"]}: {ex.Message}";
            });
        });
    }

    private async Task UpdateAutostartAsync(bool enabled)
    {
        var result = await _autostartService.SetEnabledAsync(enabled);
        if (!result.Succeeded)
        {
            RunStatus = $"{Localizer["StatusAutostartUpdateFailed"]}: {result.Error}";
            AutostartEnabled = !enabled;
            return;
        }

        _config.AutostartEnabled = enabled;
        await SaveConfigAsync();
        RunStatus = Localizer["StatusAutostartUpdated"];
    }

    private async Task UpdateLanguageAsync(string languageCode)
    {
        await Localizer.SetLanguageAsync(languageCode);
        _config.Language = Localizer.CurrentLanguage;
        SelectedLanguage = Localizer.CurrentLanguage;
        await SaveConfigAsync();
        RunStatus = Localizer["StatusLanguageUpdated"];
    }
}
