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
    private readonly IProfileStorageService _storageService;
    private readonly IProfileLauncherService _launcherService;
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    [ObservableProperty]
    private ProfileViewModel? _selectedProfile;

    [ObservableProperty]
    private LaunchItemViewModel? _selectedItem;

    [ObservableProperty]
    private string _runStatus = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<ProfileViewModel> Profiles { get; } = [];

    public ObservableCollection<string> RunErrors { get; } = [];

    public LaunchMode[] LaunchModes { get; } = Enum.GetValues<LaunchMode>();

    public ItemType[] ItemTypes { get; } = Enum.GetValues<ItemType>();

    public bool HasSelectedProfile => SelectedProfile is not null;

    public bool HasSelectedItem => SelectedItem is not null;

    public MainWindowViewModel(IProfileStorageService storageService, IProfileLauncherService launcherService)
    {
        _storageService = storageService;
        _launcherService = launcherService;

        Profiles.CollectionChanged += OnProfilesCollectionChanged;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var profileModels = await _storageService.LoadAsync();
            foreach (var model in profileModels)
            {
                var profile = new ProfileViewModel(model);
                Profiles.Add(profile);
            }

            SelectedProfile = Profiles.FirstOrDefault();
            RunStatus = "Profiles loaded";
        }
        catch (Exception ex)
        {
            RunStatus = $"Failed to load profiles: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddProfile()
    {
        var nextNumber = Profiles.Count + 1;
        var profile = new ProfileViewModel
        {
            Name = $"Profile {nextNumber}"
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

        var profileToRemove = SelectedProfile;
        Profiles.Remove(profileToRemove);

        SelectedProfile = Profiles.FirstOrDefault();
        SelectedItem = null;

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
            Label = "New Item",
            Path = string.Empty,
            Type = ItemType.App
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
        SelectedItem = null;
        TriggerAutoSave();
    }

    private bool CanRemoveItem()
    {
        return SelectedProfile is not null && SelectedItem is not null;
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
            RunStatus = "Running profile...";

            var result = await _launcherService.RunAsync(SelectedProfile.ToModel());
            foreach (var error in result.Errors)
            {
                RunErrors.Add(error.ToString());
            }

            RunStatus = $"Run completed - Total: {result.Total}, Succeeded: {result.Succeeded}, Failed: {result.Failed}";
        }
        catch (Exception ex)
        {
            RunStatus = $"Run failed: {ex.Message}";
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

    private void OnProfilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var profile in e.NewItems.OfType<ProfileViewModel>())
            {
                AttachProfile(profile);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var profile in e.OldItems.OfType<ProfileViewModel>())
            {
                DetachProfile(profile);
            }
        }
    }

    private void AttachProfile(ProfileViewModel profile)
    {
        profile.Changed += OnProfileChanged;
    }

    private void DetachProfile(ProfileViewModel profile)
    {
        profile.Changed -= OnProfileChanged;
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        TriggerAutoSave();
    }

    private void TriggerAutoSave()
    {
        SaveProfilesAsync().SafeFireAndForget(ex =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RunStatus = $"Auto-save failed: {ex.Message}";
            });
        });
    }

    private async Task SaveProfilesAsync()
    {
        await _saveSemaphore.WaitAsync();
        try
        {
            var profileModels = Profiles.Select(profile => profile.ToModel()).ToList();
            await _storageService.SaveAsync(profileModels);
        }
        finally
        {
            _saveSemaphore.Release();
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
    }
}
