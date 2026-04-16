using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenMyDear.Wpf.Models;
using OpenMyDear.Wpf.Services;

namespace OpenMyDear.Wpf.ViewModels;

public partial class InstalledAppsPickerViewModel : ObservableObject
{
    private readonly IReadOnlyList<InstalledAppModel> _allApps;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private InstalledAppModel? _selectedApp;

    public ObservableCollection<InstalledAppModel> FilteredApps { get; } = [];

    public ILocalizationService Localizer { get; }

    public InstalledAppsPickerViewModel(IReadOnlyList<InstalledAppModel> apps, ILocalizationService localizationService)
    {
        _allApps = apps;
        Localizer = localizationService;
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filter = SearchText?.Trim();
        var filtered = string.IsNullOrWhiteSpace(filter)
            ? _allApps
            : _allApps.Where(app =>
                app.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                app.ExecutablePath.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

        FilteredApps.Clear();
        foreach (var app in filtered)
        {
            FilteredApps.Add(app);
        }

        if (SelectedApp is null || !FilteredApps.Contains(SelectedApp))
        {
            SelectedApp = FilteredApps.FirstOrDefault();
        }
    }
}
