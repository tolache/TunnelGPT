using Telegram.Bot.Types;

namespace TunnelGPT.Core.Interfaces;

public interface ITelegramMessageSender
{
    Task SendMessageAsync(ChatId chatId, string message);
}