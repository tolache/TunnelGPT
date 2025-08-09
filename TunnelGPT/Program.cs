using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TunnelGPT.Core;
using TunnelGPT.Core.Interfaces;
using TunnelGPT.Infrastructure.Configuration;
using TunnelGPT.Infrastructure.Messaging;
using TunnelGPT.Infrastructure.Middleware;
using TunnelGPT.Infrastructure.OpenAi;

namespace TunnelGPT;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.FullName,
            ContentRootPath = GetContentRootPath(),
        });
        AppSettings appSettings = AppSettings.LoadFromConfiguration(builder.Configuration);
        builder.Services.AddSingleton(appSettings);
        builder.Services.AddLogging();
        builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(appSettings.TelegramBotToken));
        builder.Services.AddSingleton<IChatService>(_ => new OpenAiChatService(appSettings));
        builder.Services.AddTransient<ITelegramMessageSender, TelegramMessageSender>();
        builder.Services.AddScoped<UpdateProcessor>();
        WebApplication app = builder.Build();
        app.UseMiddleware<TelegramWebhookValidation>();
        app.MapGet("/", GenerateHomeMessage);
        app.MapPost("/", HandlePostRequest);
        app.Run();
    }

    private static string GetContentRootPath()
    {
        return Path.GetDirectoryName(typeof(Program).Assembly.Location) 
               ?? throw new InvalidOperationException("Could not determine content root path.");
    }

    private static string GenerateHomeMessage()
    {
        return $"TunnelGPT {GetProductVersion()} is running!";
    }

    private static string GetProductVersion()
    {
        string version = Assembly.GetAssembly(typeof(Program))?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown version";
        return version;
    }

    private static async Task<IResult> HandlePostRequest(HttpRequest request, ILogger<Program> logger, UpdateProcessor updateProcessor)
    {
        if (request.ContentLength is null or 0)
        {
            logger.LogWarning("Received an empty request body.");
            return Results.BadRequest("Request body must not be empty.");
        }

        if (!request.HasJsonContentType())
        {
            logger.LogWarning("Received a POST request without the 'Content-Type: application/json' header.");
            return Results.BadRequest("Invalid content type. Expected application/json.");
        }

        try
        {
            Update? update = await request.ReadFromJsonAsync<Update>();
            if (update is null) return Results.BadRequest("Invalid request. Payload must be an Update object.");
            await updateProcessor.ProcessUpdateAsync(update);
            return Results.Ok();
        }
        catch (System.Text.Json.JsonException e)
        {
            return Results.BadRequest("Failed to parse payload JSON. Reason: " + e.Message);
        }
        catch (ArgumentNullException e)
        {
            return Results.BadRequest("Invalid request. Reason: " + e.Message);
        }
    }
}
