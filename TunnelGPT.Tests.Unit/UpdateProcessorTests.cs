using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core;
using TunnelGPT.Core.Interfaces;

namespace TunnelGPT.Tests.Unit;

public class UpdateProcessorTests
{
    private readonly Mock<ILogger<UpdateProcessor>> _loggerMock = new();
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly UpdateProcessor _updateProcessor;

    public UpdateProcessorTests()
    {
        Mock<ITelegramMessageSender> messageSenderMock = new();
        _updateProcessor = new UpdateProcessor(_loggerMock.Object, _chatServiceMock.Object, messageSenderMock.Object);
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

        // Act
        await _updateProcessor.ProcessUpdateAsync(update);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
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
        _loggerMock.Verify(
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
