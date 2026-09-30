using System.Windows;
using System.Windows.Controls;
using FileSurge.Infrastructure;
using FileSurge.ViewModels;

namespace FileSurge;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();

        var settings = new SettingsService();
        settings.Load();

        _vm = new MainViewModel(settings);
        DataContext = _vm;
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string key)
            _vm.Navigate(key);
    }
}