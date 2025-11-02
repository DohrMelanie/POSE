using AppServices;
using AppServices.Importer;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace ImporterTests;

public class DatabaseWriterTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task ClearAllAsync_RemovesAllTodoItems()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.TodoItems.AddRange(
                new TodoItem { Assignee = "Test1", Title = "Test1" },
                new TodoItem { Assignee = "Test2", Title = "Test2" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TodoItemsImportDatabaseWriter(context);
            await writer.ClearAllAsync();
        }


        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            // Assert
            var count = await context.TodoItems.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task WriteDummiesAsync_AddsDummiesToDatabase()
    {
        // Arrange
        var dummies = new List<TodoItem>
        {
            new() { Assignee = "Test1", Title = "Test1" },
            new() { Assignee = "Test2", Title = "Test2" }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TodoItemsImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.WriteTodoItemsAsync(dummies);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.TodoItems.CountAsync();
            Assert.Equal(2, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_CommitSucceeds()
    {
        // Arrange
        var dummies = new[]
        { 
            new TodoItem { Assignee = "Test", Title = "Test" }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TodoItemsImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.BeginTransactionAsync();
            await writer.WriteTodoItemsAsync(dummies);
            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.TodoItems.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_RollbackSucceeds()
    {
        // Arrange
        var dummies = new TodoItem[]
        {
            new() { Assignee = "Test", Title = "Test" }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TodoItemsImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.BeginTransactionAsync();
            await writer.WriteTodoItemsAsync(dummies);
            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.TodoItems.CountAsync();
            Assert.Equal(0, count);
        }
    }
}
