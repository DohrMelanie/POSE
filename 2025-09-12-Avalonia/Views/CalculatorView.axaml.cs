using _2025_09_12_Avalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace _2025_09_12_Avalonia.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
        DataContext = new CalculatorViewModel(); // binding not possible without
    }
}