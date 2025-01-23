using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using TunnelGPT.Core;
using TunnelGPT.Infrastructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TunnelGPT;

public class Function
{
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly ChatClient _openAiClient;
    private readonly ITelegramBotClient _telegramClient;

    public Function()
    {
        _dynamoDbContext = DependencyFactory.CreateDynamoDbContext();
        _openAiClient = DependencyFactory.CreateOpenAiClient();
        _telegramClient = DependencyFactory.CreateTelegramClient();
    }

    public Function(IDynamoDBContext dynamoDbContext, ChatClient openAiClient, ITelegramBotClient telegramClient)
    {
        _dynamoDbContext = dynamoDbContext;
        _openAiClient = openAiClient;
        _telegramClient = telegramClient;
    }
    
    /// <summary>A simple function that queries Telegram and OpenAI APIs</summary>
    /// <param name="update">A Telegram update object.</param>
    /// <param name="context">The ILambdaContext providing methods for logging and describing the Lambda environment.</param>
    /// <exception cref="ArgumentNullException"> <paramref name="update"/> is null.</exception>
    /// <returns></returns>
    public async Task<Response> FunctionHandler(Update update, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(update);
        
        ILambdaLogger logger = context.Logger;
        UpdateProcessor updateProcessor = new(
            dynamoDbContext: _dynamoDbContext,
            logger: context.Logger,
            openAiClient: _openAiClient,
            telegramClient: _telegramClient
            );

        try
        {
            await updateProcessor.ProcessUpdateAsync(update);
            return new Response("Success", "Update processed successfully.");
        }
        catch (Exception e)
        {
            string errorMessage = $"Failed to process update: {e.Message}";
            logger.LogError(errorMessage);
            return new Response("Error", errorMessage);
        }
    }
    
    public record Response(string Status, string Message);
}
