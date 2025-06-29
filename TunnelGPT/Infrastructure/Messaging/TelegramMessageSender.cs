using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core.Interfaces;

namespace TunnelGPT.Infrastructure.Messaging;

public class TelegramMessageSender(ITelegramBotClient telegramClient) : ITelegramMessageSender
{
    public async Task SendMessageAsync(ChatId chatId, string message)
    {
        await telegramClient.SendMessage(chatId, message, ParseMode.Markdown);
    }
}