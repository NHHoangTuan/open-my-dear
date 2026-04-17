using System.Windows;

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
}
