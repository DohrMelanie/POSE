using System.Net;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;
using Xunit;

namespace WebApiTests;

public class LaufbewerbeIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    // Example record type for deserializing API responses — define your own as needed.
    private record LaufkategorieResponse(int Id, string Name);

    /// <summary>
    /// Example integration test: verifies that GET /laufkategorien returns the seeded categories.
    /// Use this as a template for your own tests.
    /// </summary>
    [Fact]
    public async Task Get_Laufkategorien_ReturnsSeededData()
    {
        var client = fixture.App.CreateHttpClient("webapi");

        var kategorien = await client.GetFromJsonAsync<List<LaufkategorieResponse>>("/laufkategorien");

        Assert.NotNull(kategorien);
        Assert.True(kategorien.Count >= 4);
        Assert.Contains(kategorien, k => k.Name == "Straßenlauf");
    }
    

    [Fact]
    public async Task GetLaufbewerbe_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/laufbewerbe");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLaufbewerbe_WithCategoryFilter_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/laufbewerbe?categoryId=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostLaufbewerbe_ReturnsOk()
    {
        // Act
        var dto = new
        {
            Name = "",
            Category = new
            {
                Id = 1,
                Name = "Straßenlauf"
            },
            Length = 10m,
            Place = "Hi"
        };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/laufbewerbe", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
