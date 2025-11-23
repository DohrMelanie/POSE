namespace AppServices.Importer;

/// <summary>
/// Interface for importing data from CSV files
/// </summary>
public interface ITodoItemsImporter
{
    /// <summary>
    /// Imports data from a CSV file
    /// </summary>
    /// <param name="txtFilePath">Path to the TXT file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of records imported</returns>
    Task<int> ImportFromCsvAsync(string txtFilePath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing data from CSV files
/// </summary>
public class TodoItemsImporter(IFileReader fileReader, ITodoItemsTxtParser txtParser, ITodoItemsImportDatabaseWriter databaseWriter) : ITodoItemsImporter
{
    public async Task<int> ImportFromCsvAsync(string txtFilePath, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Clear existing data
            await databaseWriter.ClearAllAsync();

            // Read TXT file
            var txtContent = await fileReader.ReadAllTextAsync(txtFilePath);

            // Parse TXT content
            var items = txtParser.ParseTxt(txtContent).ToList();

            // Write to database
            await databaseWriter.WriteTodoItemsAsync(items);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return items.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
