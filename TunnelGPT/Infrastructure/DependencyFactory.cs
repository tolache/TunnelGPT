using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using OpenAI.Chat;
using Telegram.Bot;

namespace TunnelGPT.Infrastructure;

public static class DependencyFactory
{
    public static IAmazonDynamoDB CreateDynamoDbClient(string? serviceUrl = null)
    {
        AmazonDynamoDBConfig config = new();
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            config.ServiceURL = serviceUrl;
        }
        return new AmazonDynamoDBClient(config);
    }
    
    public static IDynamoDBContext CreateDynamoDbContext(IAmazonDynamoDB? dynamoDbClient = null)
    {
        dynamoDbClient ??= CreateDynamoDbClient();
        return new DynamoDBContext(dynamoDbClient);
    }

    public static ChatClient CreateOpenAiClient()
    {
        return new ChatClient("gpt-4o", EnvironmentUtils.GetOpenAiApiKey());
    }

    public static ITelegramBotClient CreateTelegramClient()
    {
        return new TelegramBotClient(EnvironmentUtils.GetTelegramBotToken());
    }
}