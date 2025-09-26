using System.Collections.ObjectModel;
using System.Linq;

namespace _01_Avalonia_Dependency_Injection.ViewModels;
public record TodoItem(string Title, bool IsDone)
{
}

public class TodoListViewModel : ViewModelBase
{
    public TodoListViewModel()
    {
        for (int i = 0; i < 8; i++)
        {
            Items.Add(new TodoItem($"Task {i+1}", false));
        }
    }

    public ObservableCollection<TodoItem> Items { get; } = [];// statt List!

    public string CurrentTitle { get; set; }
    public void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(CurrentTitle))
        {
            Items.Add(new TodoItem(CurrentTitle, false));
            CurrentTitle = string.Empty;
            OnPropertyChanged(nameof(CurrentTitle)); // damit UI aktualisiert wird
        }
    }

    public void RemoveItem(string title)
    {
        var item = Items.FirstOrDefault(i => i.Title == title);
        if (item != null)
        {
            Items.Remove(item);
        }
    }
}
