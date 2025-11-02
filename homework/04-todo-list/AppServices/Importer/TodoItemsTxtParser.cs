namespace AppServices.Importer;

/// <summary>
/// Interface for parsing CSV content into objects
/// </summary>
public interface IDummyCsvParser
{
    /// <summary>
    /// Parses CSV content into a list of Dummy objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <returns>List of parsed Dummy objects</returns>
    IEnumerable<TodoItem> ParseCsv(string csvContent);
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class TodoItemsTxtParser : IDummyCsvParser
{
    public IEnumerable<TodoItem> ParseCsv(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
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
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
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
