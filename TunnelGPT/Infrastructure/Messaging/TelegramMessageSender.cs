using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TunnelGPT.Core.Interfaces;

namespace TunnelGPT.Infrastructure.Messaging;

public class TelegramMessageSender(ITelegramBotClient telegramClient, ChatId chatId) : ITelegramMessageSender
{
    public async Task SendMessageAsync(string message)
    {
        await telegramClient.SendMessage(chatId, message, ParseMode.Markdown);
    }
}