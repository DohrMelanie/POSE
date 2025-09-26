using _01_Avalonia_Dependency_Injection.ViewModels;
using Avalonia.Controls;

namespace _01_Avalonia_Dependency_Injection.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // DataContext = new MainWindowViewModel(); // ist falsch bei DI
    }

    public MainWindow(MainWindowViewModel viewModel) : this() // parameterloser wird auch aufgerufen
    {
        DataContext = viewModel; // wird durch DI gesetzt
    }
}