using System.Text.RegularExpressions;
using Amazon.Lambda.TestUtilities;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace TunnelGPT.Tests;

public class FunctionTest
{
    [Fact]
    public void FunctionHandler_Invoked_EchoesReceivedUpdate()
    {
        // Arrange
        Function function = new();
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
        TestLambdaContext context = new();

        // Act
        Function.Response result = function.FunctionHandler(update, context);

        // Assert
        Assert.Matches(result.Status, "Success");
    }
}