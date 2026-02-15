using System.Windows;
using System.Windows.Input;
using ToadCapture.Wpf.ViewModels;

namespace ToadCapture.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ChipTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainVm vm)
        {
            return;
        }

        if (vm.LookupChipCommand.CanExecute(null))
        {
            vm.LookupChipCommand.Execute(null);
        }

        e.Handled = true;
    }

    private void Window_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainVm vm)
        {
            return;
        }

        if (vm.SaveObservationCommand.CanExecute(null))
        {
            vm.SaveObservationCommand.Execute(null);
            e.Handled = true;
        }
    }
}
