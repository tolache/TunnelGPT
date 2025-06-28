using System.ClientModel;
using OpenAI.Chat;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core.Interfaces;
using TunnelGPT.Infrastructure.Messaging;

namespace TunnelGPT.Core;

public class UpdateProcessor(
    ILogger<UpdateProcessor> logger,
    ChatClient openAiClient,
    TelegramMessageSenderFactory messageSenderFactory
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
            reply = await GenerateReplyAsync(messageText);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to generate a reply. Reason: {Message}", e.Message);
            logger.LogDebug("{Stacktrace}", e.StackTrace);
            reply = "Sorry, I couldn't generate a reply. Reason: " + Environment.NewLine +
                    e.Message + Environment.NewLine +
                    e.StackTrace;
        }
        ITelegramMessageSender telegramMessageSender = messageSenderFactory.Create(chatId);
        await telegramMessageSender.SendMessageAsync(reply);
        logger.LogInformation("Sent a reply to user '{Username}' with id '{UserId})'.", username, userId);
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

    private async Task<string> GenerateReplyAsync(string messageText)
    {
        const string systemPrompt = "You are a helpful powered by an OpenAI language model. " +
                                    "You interact with users through the TunnelGPT Telegram bot. " +
                                    "You reply in the same language as the user's question unless otherwise specified.";
        ClientResult<ChatCompletion>? completionResult = await openAiClient.CompleteChatAsync([
            new SystemChatMessage(systemPrompt), 
            new UserChatMessage(messageText)
        ]);

        if (completionResult == null)
        {
            throw new InvalidOperationException("OpenAI API returned null.");
        }
        
        ChatCompletion completion = completionResult.Value;
        return completion.Content.First()?.Text ?? "Sorry, I couldn't generate a reply. Reason: Received an empty response from AI.";
    }
    
    private record MessageData(long UserId, string Username, string MessageText, ChatId ChatId);
}