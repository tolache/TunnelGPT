using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Infrastructure.Persistence;

namespace TunnelGPT.Core;

public class UpdateProcessor(
    IDynamoDBContext dynamoDbContext, 
    // Not ILogger until https://github.com/aws/aws-lambda-dotnet/issues/1747 is fixed
    ILambdaLogger logger,
    ChatClient openAiClient, 
    ITelegramBotClient telegramClient
)
{
    private readonly DynamoDbRepository _dynamoDbRepository = new(dynamoDbContext, logger);
    
    public async Task ProcessUpdateAsync(Update update)
    {
        (long userId, string username, string messageText, ChatId chatId) = ValidateAndExtract(update);
        string reply = $"You said: {messageText}";
        
        logger.LogInformation($"Received a message from user '{username}' with id '{userId})'.");
        await _dynamoDbRepository.SaveUserAsync(userId, username);
        await telegramClient.SendMessage(chatId, reply);
        logger.LogInformation($"Sent a reply to user '{username}' with id '{userId})'.");
    }
    
    private (long userId, string username, string messageText, ChatId chatId) ValidateAndExtract(Update update)
    {
        ArgumentNullException.ThrowIfNull(update);
        
        if (update is not { Type: UpdateType.Message, Message: not null, Message.Text: not null })
        {
            const string errorMessage = "Unsupported update. Only Message updates with non-null text are supported.";
            logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        if (update.Message.From is null)
        {
            const string errorMessage = "Unsupported update. Only messages from non-null users are supported.";
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
        string username = update.Message.From.Username ?? string.Empty;
        string messageText = update.Message.Text;
        ChatId chatId = update.Message.Chat.Id;
        
        return (userId, username, messageText, chatId);
    }
}