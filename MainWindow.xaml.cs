using System.Windows;
using SerialProtocolAssistant.ViewModels;

namespace SerialProtocolAssistant;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
