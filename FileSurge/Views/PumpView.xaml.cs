using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FileSurge.ViewModels;

namespace FileSurge.Views;

public partial class PumpView : UserControl
{
    public PumpView()
    {
        InitializeComponent();
        if (DataContext is null)
            DataContext = new PumpViewModel();
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
        if (sender is Border b)
            b.Background = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0xFF, 0x9C));
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border b)
            b.Background = new SolidColorBrush(Color.FromArgb(0x08, 0x00, 0xFF, 0x9C));

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length > 0 && DataContext is PumpViewModel vm)
                vm.SetFile(files[0]);
        }
    }

    private void DropZone_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PumpViewModel vm && vm.BrowseCommand.CanExecute(null))
            vm.BrowseCommand.Execute(null);
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PumpViewModel vm) vm.Logs.Clear();
    }
}