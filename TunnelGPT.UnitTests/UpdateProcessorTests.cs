using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core;
using TunnelGPT.Core.Interfaces;
using TunnelGPT.Infrastructure.Messaging;

namespace TunnelGPT.UnitTests;

public class UpdateProcessorTests
{
    private readonly Mock<ILogger<UpdateProcessor>> _mockLogger = new();
    private readonly Mock<ChatClient> _mockOpenAiClient = new("gpt-4o", "mock_api_key");
    private readonly Mock<ITelegramBotClient> _mockTelegramBotClient = new();
    private readonly UpdateProcessor _updateProcessor;

    public UpdateProcessorTests()
    {
        var mockMessageSenderFactory = new Mock<TelegramMessageSenderFactory>(_mockTelegramBotClient.Object);
        _updateProcessor = new UpdateProcessor(_mockLogger.Object, _mockOpenAiClient.Object, mockMessageSenderFactory.Object);
    }
    
    [Fact]
    public async Task ProcessUpdate_GivenValidUpdate_CompletesWithoutError()
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
        
        const string responsePrompt = "response";
        ChatCompletion? completion = OpenAIChatModelFactory.ChatCompletion(
            role: ChatMessageRole.User,
            content: new ChatMessageContent(ChatMessageContentPart.CreateTextPart(responsePrompt)));
        PipelineResponse pipelineResponse = new Mock<PipelineResponse>().Object;
        ClientResult<ChatCompletion> completionResult = ClientResult.FromValue(completion, pipelineResponse);
        _mockOpenAiClient
            .Setup(x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()))
            .ReturnsAsync(completionResult);

        // Act
        await _updateProcessor.ProcessUpdateAsync(update);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Never()
        );
    }
    
    [Fact]
    public async Task ProcessUpdate_GivenInvalidUpdate_ReturnsErrorResponse()
    {
        // Arrange
        Update update = new();
        
        // Act
        Exception? exception = await Record.ExceptionAsync(
            () => _updateProcessor.ProcessUpdateAsync(update)
        );
        
        // Assert
        Assert.IsType<ArgumentException>(exception);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce()
        );
    }
}
