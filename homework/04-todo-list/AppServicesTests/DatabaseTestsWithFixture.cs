using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CanAddAndRetrieveDummy()
    {
        // Arrange & Act
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new TodoItem 
            { 
                Assignee = "Test Dummy",
                Title = "Test Name"
            };
            context.TodoItems.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.TodoItems.FindAsync(dummyId);
            Assert.NotNull(dummy);
            Assert.Equal("Test Dummy", dummy.Assignee);
        }
    }

    [Fact]
    public async Task CanUpdateDummy()
    {
        // Arrange
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new TodoItem 
            { 
                Assignee = "Original Name", 
                Title = "Original Title"
            };
            context.TodoItems.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.TodoItems.FindAsync(dummyId);
            Assert.NotNull(dummy);
            dummy.Assignee = "Updated Name";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.TodoItems.FindAsync(dummyId);
            Assert.NotNull(dummy);
            Assert.Equal("Updated Name", dummy.Assignee);
            Assert.Equal("Original Title", dummy.Title);
        }
    }

    [Fact]
    public async Task CanDeleteDummy()
    {
        // Arrange
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new TodoItem 
            { 
                Assignee = "To Delete", 
                Title = "To Delete"
            };
            context.TodoItems.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.TodoItems.FindAsync(dummyId);
            Assert.NotNull(dummy);
            context.TodoItems.Remove(dummy);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.TodoItems.FindAsync(dummyId);
            Assert.Null(dummy);
        }
    }

    [Fact]
    public async Task CanQueryMultipleDummies()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.TodoItems.AddRange(
                new TodoItem { Assignee = "Query Test 1", Title = "Test" },
                new TodoItem { Assignee = "Query Test 2", Title = "test" },
                new TodoItem { Assignee = "Query Test 3", Title = "test" }
            );
            await context.SaveChangesAsync();
        }

        // Act & Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummies = await context.TodoItems
                .Where(d => d.Assignee.StartsWith("Query Test") && d.Title.StartsWith('t'))
                .OrderBy(d => d.Assignee)
                .ToListAsync();

            Assert.Equal(2, dummies.Count);
            Assert.Equal("Query Test 2", dummies[0].Assignee);
            Assert.Equal("Query Test 3", dummies[1].Assignee);
        }
    }
}
