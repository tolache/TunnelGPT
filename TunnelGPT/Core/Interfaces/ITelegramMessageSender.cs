namespace TunnelGPT.Core.Interfaces;

public interface ITelegramMessageSender
{
    Task SendMessageAsync(string message);
}