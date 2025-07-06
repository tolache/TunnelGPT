using Moq;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace TunnelGPT.Tests.Common;

public static class OpenAiMockFactory
{
    public static Mock<ChatClient> CreateMockOpenAiClient()
    {
        ClientResult<ChatCompletion> completionResult = CreateMockChatCompletion();
        Mock<ChatClient> mockOpenAiClient = new("gpt-4o", "mock_api_key");
        mockOpenAiClient
            .Setup(x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()))
            .ReturnsAsync(completionResult);
        return mockOpenAiClient;
    }

    private static ClientResult<ChatCompletion> CreateMockChatCompletion()
    {
        string responsePrompt = "mock_response";
        ChatCompletion? completion = OpenAIChatModelFactory.ChatCompletion(
            role: ChatMessageRole.User,
            content: new ChatMessageContent(ChatMessageContentPart.CreateTextPart(responsePrompt))
        );
        PipelineResponse pipelineResponse = new Mock<PipelineResponse>().Object;
        ClientResult<ChatCompletion> completionResult = ClientResult.FromValue(completion, pipelineResponse);
        return completionResult;
    }
}