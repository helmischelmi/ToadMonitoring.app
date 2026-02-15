using System.Windows;
using ToadCapture.Wpf.ViewModels;

namespace ToadCapture.Wpf.Views;

public partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();
    }

    private void ScannerTestButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is StartVm vm)
        {
            vm.TriggerScannerTest();
        }
    }

    private void SourcePathBrowse_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not StartVm vm)
        {
            return;
        }

        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            vm.SourceXlsxPath = dlg.FileName;
        }
    }
}
