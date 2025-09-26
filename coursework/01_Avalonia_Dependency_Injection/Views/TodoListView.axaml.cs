using System.Collections.ObjectModel;
using _01_Avalonia_Dependency_Injection.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace _01_Avalonia_Dependency_Injection.Views;

public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }

    public TodoListView(TodoListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}