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
}