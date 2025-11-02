using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface ITodoItemsImportDatabaseWriter
{
    /// <summary>
    /// Clears all existing TodoItem records from the database
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Writes a collection of TodoItem objects to the database
    /// </summary>
    /// <param name="items">TodoItems to write</param>
    Task WriteTodoItemsAsync(IEnumerable<TodoItem> items);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}

/// <summary>
/// Implementation for writing objects to the database
/// </summary>
public class TodoItemsImportDatabaseWriter(ApplicationDataContext context) : ITodoItemsImportDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task ClearAllAsync()
    {
        await context.TodoItems.ExecuteDeleteAsync();
    }

    public async Task WriteTodoItemsAsync(IEnumerable<TodoItem> items)
    {
        await context.TodoItems.AddRangeAsync(items);
        await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }
}
