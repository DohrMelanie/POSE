using _01_Avalonia_Dependency_Injection.Views;

namespace _01_Avalonia_Dependency_Injection.ViewModels;

public class MainWindowViewModel(TodoListViewModel todoListViewModel) : ViewModelBase
{
    public TodoListViewModel TodoListViewModel { get;  } = todoListViewModel;
}
