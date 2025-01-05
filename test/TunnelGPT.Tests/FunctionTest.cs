using System.Text.RegularExpressions;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using Telegram.Bot.Types;
using Xunit.Abstractions;

namespace TunnelGPT.Tests;

public class FunctionTest
{
    [Fact]
    public void FunctionHandler_Invoked_EchoesReceivedUpdate()
    {
        // Arrange
        Function function = new();
        const string updateFromTelegram = """
                                          {
                                              "update_id": 1234567890,
                                              "message": {
                                                  "message_id": 123,
                                                  "from": {
                                                      "id": 9876543210,
                                                      "is_bot": false,
                                                      "first_name": "John",
                                                      "last_name": "Doe",
                                                      "username": "johndoe",
                                                      "language_code": "en"
                                                  },
                                                  "chat": {
                                                      "id": 9876543210,
                                                      "first_name": "John",
                                                      "last_name": "Doe",
                                                      "username": "johndoe",
                                                      "type": "private"
                                                  },
                                                  "date": 1735736394,
                                                  "text": "2025-01-01 13:59"
                                              }
                                          }
                                          """;
        Update? deserializedUpdateFromTelegram = JsonSerializer.Deserialize<Update>(updateFromTelegram);
        if (deserializedUpdateFromTelegram == null)
        {
            throw new InvalidOperationException("Failed to deserialize update from Telegram.");
        }
        TestLambdaContext context = new();
        Regex pattern = new("^Received update: {\"update_id\":1234567890,.+}");

        // Act
        string result = function.FunctionHandler(deserializedUpdateFromTelegram, context);

        // Assert
        Assert.Matches(pattern, result);
    }
}