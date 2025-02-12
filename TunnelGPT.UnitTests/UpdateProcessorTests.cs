using Moq;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TunnelGPT.UnitTests;

public class UpdateProcessorTests
{
    [Fact]
    public async Task ProcessUpdate_GivenValidUpdate_ReturnsSuccessfulResponse()
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
        throw new NotImplementedException();

        // Assert
    }
    
    [Fact]
    public async Task ProcessUpdate_GivenInvalidUpdate_ReturnsErrorResponse()
    {
        // Arrange
        Update update = new();
        
        // Act
        throw new NotImplementedException();

        // Assert
    }
}
