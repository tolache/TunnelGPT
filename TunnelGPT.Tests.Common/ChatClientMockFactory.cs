using Moq;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace TunnelGPT.Tests.Common;

public static class ChatClientMockFactory
{
    [Experimental("OPENAI001")]
    public static Mock<ChatClient> CreateChatClientMock()
    {
        ClientResult<ChatCompletion> completionResult = CreateChatCompletionMock();
        Mock<ChatClient> chatClientMock = new("gpt-5.2", "mock_api_key");
        chatClientMock
            .Setup(x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()))
            .ReturnsAsync(completionResult);
        return chatClientMock;
    }

    [Experimental("OPENAI001")]
    private static ClientResult<ChatCompletion> CreateChatCompletionMock()
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