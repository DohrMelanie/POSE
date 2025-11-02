namespace AppServices;

public class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; } = string.Empty;
    public required string Assignee { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}
