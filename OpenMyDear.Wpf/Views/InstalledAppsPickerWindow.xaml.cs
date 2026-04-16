using System.Windows;
using OpenMyDear.Wpf.ViewModels;

namespace OpenMyDear.Wpf.Views;

public partial class InstalledAppsPickerWindow : Window
{
    public InstalledAppsPickerWindow()
    {
        InitializeComponent();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not InstalledAppsPickerViewModel viewModel || viewModel.SelectedApp is null)
        {
            return;
        }

        DialogResult = true;
        Close();
    }
}
