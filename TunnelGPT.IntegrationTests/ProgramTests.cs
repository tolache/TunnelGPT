using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core.Interfaces;
using TunnelGPT.Infrastructure.Configuration;
using Moq;
using OpenAI.Chat;

namespace TunnelGPT.IntegrationTests;

public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string _correctTelegramBotSecret;

    public ProgramTests(WebApplicationFactory<Program> factory)
    {
        Mock<ChatClient> mockOpenAiClient = UnitTests.MockFactory.CreateMockOpenAiClient("mock_response");
        Mock<ITelegramMessageSender> mockTelegramSender = new();
        mockTelegramSender
            .Setup(x => x.SendMessageAsync(It.IsAny<ChatId>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor? chatClient = services
                        .SingleOrDefault(d => d.ServiceType == typeof(ChatClient)); 
                    if (chatClient != null) services.Remove(chatClient);
                    services.AddSingleton(mockOpenAiClient.Object);
                    
                    ServiceDescriptor? telegramMessageSender = services
                        .SingleOrDefault(d => d.ServiceType == typeof(ITelegramMessageSender));
                    if (telegramMessageSender != null) services.Remove(telegramMessageSender);
                    services.AddSingleton(mockTelegramSender.Object);
                });
            })
            .CreateClient();
        _correctTelegramBotSecret = factory.Server.Services.GetRequiredService<AppSettings>().TelegramBotSecret;
    }

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
        Update update = new()
        {
            Id = 101,
            Message = new Message
            {
                Id = 201,
                Date = new DateTime(2025, 1, 1),
                Text = "Hello, bot!",
                From = new User
                {
                    Id = 301,
                    FirstName = "John",
                    LastName = "Doe",
                    Username = "johndoe",
                    IsBot = false,
                    LanguageCode = "en",
                },
                Chat = new Chat
                {
                    Id = 401,
                    FirstName = "John",
                    LastName = "Doe",
                    Username = "johndoe",
                    Type = ChatType.Private,
                },
            }
        };
        JsonContent payload = JsonContent.Create(update);
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {nameof(HttpRequestHeader.ContentType), "application/json"},
                {"X-Telegram-Bot-Api-Secret-Token", _correctTelegramBotSecret}
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
                {nameof(HttpRequestHeader.ContentType), "application/json"},
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
                {nameof(HttpRequestHeader.ContentType), "application/json"},
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
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/", UriKind.Relative),
            Headers =
            {
                {nameof(HttpRequestHeader.ContentType), "application/json"},
                {"X-Telegram-Bot-Api-Secret-Token", _correctTelegramBotSecret}
            },
            Content = payload,
        };
        
        // Act
        HttpResponseMessage response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
