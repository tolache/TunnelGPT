using System.ClientModel;
using System.ClientModel.Primitives;
using Moq;
using OpenAI.Chat;

namespace TunnelGPT.UnitTests;

public class MockFactory
{
    public static Mock<ChatClient> CreateMockOpenAiClient(string desiredResponse)
    {
        string responsePrompt = desiredResponse;
        ChatCompletion? completion = OpenAIChatModelFactory.ChatCompletion(
            role: ChatMessageRole.User,
            content: new ChatMessageContent(ChatMessageContentPart.CreateTextPart(responsePrompt))
        );
        PipelineResponse pipelineResponse = new Mock<PipelineResponse>().Object;
        ClientResult<ChatCompletion> completionResult = ClientResult.FromValue(completion, pipelineResponse);
        Mock<ChatClient> mockOpenAiClient = new("gpt-4o", "mock_api_key");
        mockOpenAiClient
            .Setup(x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()))
            .ReturnsAsync(completionResult);
        return mockOpenAiClient;
    }
}