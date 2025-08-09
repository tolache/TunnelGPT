namespace TunnelGPT.Core.Interfaces;

public interface IChatService
{
    string Model { get; }
    Task<string> GenerateReplyAsync(string systemPrompt, string userMessage);
}