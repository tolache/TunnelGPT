using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TunnelGPT.Utils;

public interface IUpdateProcessor
{
    Task ProcessUpdateAsync(Update update);
}

public class UpdateProcessor(ITelegramBotClient telegramClient, ChatClient openAiClient) : IUpdateProcessor
{
    private readonly ITelegramBotClient _telegramClient = telegramClient;
    private readonly ChatClient _openAiClient = openAiClient;

    public Task ProcessUpdateAsync(Update update)
    {
        ArgumentNullException.ThrowIfNull(update);
        Console.WriteLine($"Update type is: '{update.Type}'");
        if (update is { Type: UpdateType.Message, Message: not null })
        {
            Console.WriteLine($"Message id '{update.Message.Id}' from '{update.Id}' is processed.");
        }
        else
        {
            Console.WriteLine("Can only process text message updates.");
        }
        return Task.CompletedTask;
    }
}