using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using OpenAI.Chat;
using Telegram.Bot;

namespace TunnelGPT.Infrastructure;

public static class DependencyFactory
{
    public static IDynamoDBContext CreateDynamoDbContext()
    {
        return new DynamoDBContext(new AmazonDynamoDBClient());
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