using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TunnelGPT;

public class Function
{
    /// <summary>
    /// A simple function that queries Telegram API
    /// </summary>
    /// <param name="input">A Telegram update object.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(JsonObject? input, ILambdaContext context)
    {
        string message;
        if (input == null)
        {
            message = "Error! Received invalid payload: update is null";
            LambdaLogger.Log(message);
            return message;
        }
        message = $"Received update: {JsonSerializer.Serialize(input)}";
        LambdaLogger.Log(message);
        return message;
    }

    private static string GetBotToken()
    {
        const string telegramBotTokenEnvVar = "TELEGRAM_BOT_TOKEN";
        string? botToken = Environment.GetEnvironmentVariable(telegramBotTokenEnvVar);
        if (string.IsNullOrEmpty(botToken))
        {
            throw new InvalidOperationException($"Environment variable {telegramBotTokenEnvVar} is not set.");
        }
        return botToken;
    }
}
