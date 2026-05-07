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
    public async Task GetCompetitions_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/laufbewerbe");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetCompetitionById_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/laufbewerbe?id=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task PostCompetition_ReturnsOk()
    {
        // Act
        // Arrange
        var dto = new
        {
            Name = "Test",
            Place = "Test",
            Category = new
            {
                Id = 1,
                Name = "Name should be the same"
            },
            Length = 0.1m,
            Date = DateOnly.FromDateTime(DateTime.Now),
        };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/laufbewerbe", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
