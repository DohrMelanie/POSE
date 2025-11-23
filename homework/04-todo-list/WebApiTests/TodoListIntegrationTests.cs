using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class TodoListIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetAllTodoItems_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostTodoItem_ReturnsCreated()
    {
        var todoItem = new
        {
            Assignee = "Rainer",
            Title = "Hallo",
        };
        
        var response = await fixture.HttpClient.PostAsJsonAsync("/items", todoItem);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PutTodoItem_ReturnsNoContent()
    {
        var todoItem = new
        {
            Assignee = "Rainer",
            Title = "Hallo",
        };
        var response = await fixture.HttpClient.PutAsJsonAsync("/items/8", todoItem);
        Assert.True(response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNoContent()
    {
        var todoItem = new
        {
            Id = 1,
            Assignee = "Rainer",
            Title = "Updated Name",
        };
        var response = await fixture.HttpClient.DeleteAsync("/items/1");
    }
}