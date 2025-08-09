using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core.Interfaces;

namespace TunnelGPT.Core;

public class UpdateProcessor(
    ILogger<UpdateProcessor> logger,
    IChatService chatService,
    ITelegramMessageSender telegramMessageSender
)
{
    public async Task ProcessUpdateAsync(Update update)
    {
        MessageData messageData = ExtractMessageData(update);
        long userId = messageData.UserId;
        string username = messageData.Username;
        string messageText = messageData.MessageText;
        ChatId chatId = messageData.ChatId;
        
        logger.LogInformation("Received a message from user '{Username}' with id '{UserId}'.", username, userId);
        
        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        
        string reply;
        try
        {
            reply = await GetReplyFromLlm(messageText);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to generate a reply. Reason: {Message}", e.Message);
            logger.LogDebug("{Stacktrace}", e.StackTrace);
            reply = "Sorry, I couldn't generate a reply. Reason: " + Environment.NewLine +
                    e.Message + Environment.NewLine +
                    e.StackTrace;
        }
        await telegramMessageSender.SendMessageAsync(chatId, reply);
        logger.LogInformation("Sent a reply to user '{Username}' with id '{UserId}'.", username, userId);
    }
    
    private MessageData ExtractMessageData(Update update)
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
        ChatId chatId = new(update.Message.Chat.Id);
        
        return new MessageData(userId, username, messageText, chatId);
    }

    private async Task<string> GetReplyFromLlm(string messageText)
    {
        string systemPrompt = $"You are TunnelGPT, a helpful Telegram bot. " +
                              $"You relay user messages to the {chatService.Model} LLM and return its replies.";

        try
        {
            return await chatService.GenerateReplyAsync(systemPrompt, messageText);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to generate a reply. Reason: {Message}", e.Message);
            logger.LogDebug("{Stacktrace}", e.StackTrace);
            string errorReplyMessage = 
                "Sorry, I couldn't generate a reply. " +
                "If this error reproduces consistently, please report it to the bot admin. " +
                "Reason: " + Environment.NewLine + 
                e.Message + Environment.NewLine + 
                e.StackTrace;
            return errorReplyMessage;
        }
    }
    
    private record MessageData(long UserId, string Username, string MessageText, ChatId ChatId);
}