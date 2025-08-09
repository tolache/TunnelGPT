using System.ClientModel;
using OpenAI.Chat;
using TunnelGPT.Core.Interfaces;
using TunnelGPT.Infrastructure.Configuration;

namespace TunnelGPT.Infrastructure.OpenAi;

public class OpenAiChatService(AppSettings settings) : IChatService
{
    private readonly ChatClient _openAiChatClient = new(settings.OpenAiModel, settings.OpenAiApiKey);
    public string Model { get; } = settings.OpenAiModel;

    public async Task<string> GenerateReplyAsync(string systemPrompt, string userMessage)
    {
        ClientResult<ChatCompletion>? completionResult = await _openAiChatClient.CompleteChatAsync([
            new SystemChatMessage(systemPrompt), 
            new UserChatMessage(userMessage)
        ]);

        if (completionResult == null)
        {
            throw new InvalidOperationException("OpenAI API returned null.");
        }
        
        ChatCompletion completion = completionResult.Value;
        return completion.Content.First()?.Text ?? "Sorry, I couldn't generate a reply. Reason: Received an empty response from LLM.";
    }
}