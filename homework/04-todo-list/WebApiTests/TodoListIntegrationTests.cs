using System.Net;

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
        var response = await fixture.HttpClient.PostAsync("/items", new StringContent(""));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public 
}