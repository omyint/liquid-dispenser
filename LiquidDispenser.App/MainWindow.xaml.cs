using System.Windows;

namespace LiquidDispenser.App;

public partial class MainWindow : Window
{
    // Inject the ViewModel through the constructor
    public MainWindow(LiquidDispenser.App.ViewModels.MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}