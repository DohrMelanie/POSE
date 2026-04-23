using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class OrderIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await fixture.HttpClient.GetAsync($"/orders/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ValidateOrder_Valid_Returns200()
    {
        var data = "A";
        var response = await fixture.HttpClient.GetAsync($"/bracelets/validate/{data}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var details = await response.Content.ReadFromJsonAsync<ValidationResult>();
        Assert.NotNull(details);
        Assert.Null(details.Error);
        Assert.False(details.MixedColorWarning);
        Assert.Equal(1m, details.Cost);
    }

    public record ValidationResult(
        string? Error,
        bool MixedColorWarning,
        decimal? Cost);
}
