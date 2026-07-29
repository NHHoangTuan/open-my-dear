using System.Windows;
using OpenMyDear.Wpf.ViewModels;

namespace OpenMyDear.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnOpenSettingsClicked(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            DataContext = DataContext
        };

        settingsWindow.ShowDialog();
    }

    private void OnEditItemClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: LaunchItemViewModel item }
            || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.EditItemCommand.Execute(item);

        var editorWindow = new ItemEditorWindow
        {
            Owner = this,
            DataContext = viewModel
        };

        editorWindow.ShowDialog();

        if (item.IsEditing)
        {
            viewModel.EditItemCommand.Execute(item);
        }
    }
}
