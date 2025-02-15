using Microsoft.AspNetCore.Mvc.Testing;

namespace TunnelGPT.IntegrationTests;

public class ProgramTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetRoot_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/");
        
        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal("TunnelGPT is running!", content);
    }

    [Fact]
    public async Task PostWithoutSecret_ReturnsUnauthorized()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task PostWithIvalidPayload_ReturnsBadRequest()
    {
        throw new NotImplementedException();
    }
}
