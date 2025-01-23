using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TunnelGPT.Core;

public class UpdateProcessor(
    IDynamoDBContext dynamoDbContext, 
    ILambdaLogger logger, // Not ILogger until https://github.com/aws/aws-lambda-dotnet/issues/1747 is fixed
    ChatClient openAiClient, 
    ITelegramBotClient telegramClient)
{

    /// <summary> It handles the messages and sends the replies.</summary>
    /// <param name="update">A Telegram update object.</param>
    /// <exception cref="ArgumentNullException"> <paramref name="update"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when unsupported updates are encountered. </exception>
    /// <returns></returns>
    public async Task ProcessUpdateAsync(Update update)
    {
        ArgumentNullException.ThrowIfNull(update);
        
        if (update is not { Type: UpdateType.Message, Message: not null })
        {
            const string errorMessage = "Unsupported update. Message must not be null.";
            logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        if (update.Message.From is null)
        {
            const string errorMessage = "Unsupported update. User must not be null.";
            logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        if (update.Message.From.IsBot)
        {
            const string errorMessage = "Unsupported update. User must not be a bot.";
            logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }
        
        long userId = update.Message.From.Id;
        string? username = update.Message.From.Username;
        string? message = update.Message.Text;
        string reply = $"You said: {message}";
        
        logger.LogInformation($"Received a message from user '{username}' with id '{userId})'.");
        
        // Write to DynamoDB
        TunnelGPT.Core.DataModel.User tunnelGptUser = new()
        {
            UserId = userId,
            Username = username,
        };
        try
        {
            await dynamoDbContext.SaveAsync(tunnelGptUser);
            logger.LogInformation($"Saved user '{username}' with id '{userId})' to the database.");
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to write user to the database. Reason: {e.Message}");
            logger.LogDebug(e.StackTrace);
            throw;
        }
        
        await telegramClient.SendMessage(update.Message.Chat.Id, reply);
        logger.LogInformation($"Sent a reply to user '{username}' with id '{userId})'.");
    }
}