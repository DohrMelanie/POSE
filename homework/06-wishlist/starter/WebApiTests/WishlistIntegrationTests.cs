using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebApiTests;

public class WishlistIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string WishlistName = "XMasAtStropeks";
    private const string ParentPin = "9JX7KM";
    private const string ChildPin = "TR4GQZ";
    
    [Fact]
    public async Task GetGiftCategories_ReturnsCategories_WithoutPin()
    {
        var response = await fixture.HttpClient.GetAsync("/gift-categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(categories);
        Assert.Contains("Books", categories!);
    }
    

    [Fact]
    public async Task VerifyPin_IsCaseInsensitive()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/verify-pin/{WishlistName}", new VerifyPinRequestDto(ParentPin.ToLowerInvariant()), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task VerifyPin_WithParentPin_ReturnsParentRole()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/verify-pin/{WishlistName}", new VerifyPinRequestDto(ParentPin), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyPinResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("parent", body!.Role);
    }
    
    
    [Fact]
    public async Task VerifyPin_WithChildPin_ReturnsChildRole()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/verify-pin/{WishlistName}", new VerifyPinRequestDto(ParentPin), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyPinResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("parent", body!.Role);
    }
}

public record VerifyPinRequestDto(string Pin);
public record VerifyPinResponseDto(string Role);
