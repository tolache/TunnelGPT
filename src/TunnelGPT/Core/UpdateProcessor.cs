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
        MessageData messageData = ValidateAndExtract(update);
        long userId = messageData.UserId;
        string username = messageData.Username;
        string messageText = messageData.MessageText;
        ChatId chatId = messageData.ChatId;
        
        logger.LogInformation($"Received a message from user '{username}' with id '{userId})'.");
        await _dynamoDbRepository.SaveUserAsync(userId, username);
        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        
        string reply;
        try
        {
            reply = await GenerateReplyAsync(messageText);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to generate a reply. Reason: {e.Message}");
            logger.LogDebug(e.StackTrace);
            reply = "Sorry, I couldn't generate a reply. Reason: " + Environment.NewLine +
                    e.Message + Environment.NewLine +
                    e.StackTrace;
        }
        await telegramClient.SendMessage(chatId, reply);
        logger.LogInformation($"Sent a reply to user '{username}' with id '{userId})'.");
    }
    
    private MessageData ValidateAndExtract(Update update)
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
        
        return new MessageData(userId, username, messageText, chatId);
    }

    private async Task<string> GenerateReplyAsync(string messageText)
    {
        const string systemPrompt = "You are a helpful assistant. " +
                                    "Always provide a short answer unless the user explicitly asks for a detailed one. " +
                                    "Reply in the same language as the user's question.";
        ChatCompletion completion = await openAiClient.CompleteChatAsync([
            new SystemChatMessage(systemPrompt), 
            new UserChatMessage(messageText)
        ]);
        return completion.Content.First()?.Text ?? "Sorry, I couldn't generate a reply. Reason: Empty response.";
    }
    
    private record MessageData(long UserId, string Username, string MessageText, ChatId ChatId);
}