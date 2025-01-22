using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using OpenAI.Chat;
using Telegram.Bot;

namespace TunnelGPT.Core;

public class UpdateProcessorDependencies
{
    public IDynamoDBContext DynamoDbContext { get; }
    public ILambdaLogger Logger { get; } // not ILogger because https://github.com/aws/aws-lambda-dotnet/issues/1747
    public ChatClient OpenAiClient { get; }
    public ITelegramBotClient TelegramClient { get; }

    public UpdateProcessorDependencies(
        IDynamoDBContext dynamoDbContext,
        ILambdaLogger logger,
        ChatClient openAiClient,
        ITelegramBotClient telegramClient)
    {
        DynamoDbContext = dynamoDbContext;
        Logger = logger;
        OpenAiClient = openAiClient;
        TelegramClient = telegramClient;
    }
}