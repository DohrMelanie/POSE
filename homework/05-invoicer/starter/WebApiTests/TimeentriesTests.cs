using System.Net;

namespace WebApiTests;

public class TimeentriesTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetEmployees_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/employees");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeEntries_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/timeentries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
