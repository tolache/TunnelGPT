using Telegram.Bot;
using Telegram.Bot.Types;
using TunnelGPT.Core.Interfaces;

namespace TunnelGPT.Infrastructure.Messaging;

public class TelegramMessageSenderFactory(ITelegramBotClient telegramClient)
{
    public ITelegramMessageSender Create(ChatId chatId)
    {
        return new TelegramMessageSender(telegramClient, chatId);
    }
}