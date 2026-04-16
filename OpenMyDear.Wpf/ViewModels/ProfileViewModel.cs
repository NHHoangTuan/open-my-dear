using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private LaunchMode _launchMode;

    [ObservableProperty]
    private double _delaySeconds;

    public ObservableCollection<LaunchItemViewModel> Items { get; }

    public event EventHandler? Changed;

    public ProfileViewModel()
    {
        _id = Guid.NewGuid().ToString();
        _name = "New Profile";
        _launchMode = LaunchMode.Parallel;
        _delaySeconds = 2.0;
        Items = [];

        WireItemCollectionEvents();
    }

    public ProfileViewModel(ProfileModel model)
    {
        _id = string.IsNullOrWhiteSpace(model.Id) ? Guid.NewGuid().ToString() : model.Id;
        _name = model.Name;
        _launchMode = model.LaunchMode;
        _delaySeconds = model.DelaySeconds;
        Items = new ObservableCollection<LaunchItemViewModel>(model.Items.Select(item => new LaunchItemViewModel(item)));

        WireItemCollectionEvents();
    }

    public ProfileModel ToModel()
    {
        return new ProfileModel
        {
            Id = Id,
            Name = Name,
            LaunchMode = LaunchMode,
            DelaySeconds = DelaySeconds,
            Items = Items.Select(item => item.ToModel()).ToList()
        };
    }

    private void WireItemCollectionEvents()
    {
        Items.CollectionChanged += OnItemsCollectionChanged;

        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<LaunchItemViewModel>())
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<LaunchItemViewModel>())
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnNameChanged(string value)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnLaunchModeChanged(LaunchMode value)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnDelaySecondsChanged(double value)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
