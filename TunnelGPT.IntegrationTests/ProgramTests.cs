using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using TunnelGPT.Infrastructure.Configuration;

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
        Assert.Matches("TunnelGPT.*is running!", content);
    }

    [Fact]
    public async Task PostWithCorrectSecretAndData_ReturnsOk()
    {
        // Arrange
        JsonContent payload = JsonContent.Create(new Update());
        string telegramBotSecret = factory.Server.Services.GetRequiredService<AppSettings>().TelegramBotSecret;
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {HttpRequestHeader.ContentType.ToString(), "application/json"},
                {"X-Telegram-Bot-Api-Secret-Token", telegramBotSecret}
            },
            Content = payload,
        };
        
        // Act
        HttpResponseMessage response = await _client.SendAsync(request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostWithoutSecret_ReturnsUnauthorized()
    {
        // Arrange
        JsonContent payload = JsonContent.Create(new Update());
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {HttpRequestHeader.ContentType.ToString(), "application/json"},
            },
            Content = payload,
        };
        
        // Act
        HttpResponseMessage response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task PostWitIncorrectSecret_ReturnsUnauthorized()
    {
        // Arrange
        JsonContent payload = JsonContent.Create(new Update());
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {HttpRequestHeader.ContentType.ToString(), "application/json"},
                {"X-Telegram-Bot-Api-Secret-Token", "incorrect_value"},
            },
            Content = payload,
        };
        
        // Act
        HttpResponseMessage response = await _client.PostAsync("/", payload);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostWithInvalidPayload_ReturnsBadRequest()
    {
        // Arrange
        JsonContent payload = JsonContent.Create("");
        string telegramBotSecret = factory.Server.Services.GetRequiredService<AppSettings>().TelegramBotSecret;
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {HttpRequestHeader.ContentType.ToString(), "application/json"},
                {"X-Telegram-Bot-Api-Secret-Token", telegramBotSecret}
            },
            Content = payload,
        };
        
        // Act
        HttpResponseMessage response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
