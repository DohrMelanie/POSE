namespace AppServices.Importer;

/// <summary>
/// Interface for parsing CSV content into objects
/// </summary>
public interface ITodoItemsTxtParser
{
    /// <summary>
    /// Parses TXT content into a list of todos
    /// </summary>
    /// <param name="txtContent">TXT content as string</param>
    /// <returns>List of parsed Dummy objects</returns>
    IEnumerable<TodoItem> ParseTxt(string txtContent);
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class TodoItemsTxtParser : ITodoItemsTxtParser
{
    public IEnumerable<TodoItem> ParseTxt(string txtContent)
    {
        var lines = txtContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        if (lines.Length == 0)
        {
            throw new InvalidOperationException("TXT content is empty.");
        }

        var items = new List<TodoItem>();
        var assignee = string.Empty;
        
        // Parse data
        foreach (var line in lines)
        {
            if (line.StartsWith("Assignee: "))
            {
                assignee = line.Replace("Assignee: ", "");
            } else if (line.StartsWith("Todos:") || line.StartsWith("---")) 
            {
                continue;
            } else if (line.StartsWith('*')) 
            {
                items.Add(new TodoItem
                {
                    Assignee = assignee,
                    Title = line.Replace("* ", "")
                });
            }
        }

        if (assignee != string.Empty && !items.Any())
        {
            throw new InvalidOperationException("Insufficient content");
        }

        return items;
    }
}
