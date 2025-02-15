using Telegram.Bot.Types;
using TunnelGPT.Infrastructure.Configuration;
using TunnelGPT.Infrastructure.Middleware;

namespace TunnelGPT;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddLogging();
        AppSettings appSettings = AppSettings.LoadFromConfiguration(builder.Configuration);
        builder.Services.AddSingleton(appSettings);
        WebApplication app = builder.Build();
        
        app.UseHttpsRedirection();
        app.UseMiddleware<TelegramWebhookValidation>();
        app.MapGet("/", () => "TunnelGPT is running!");
        app.MapPost("/", HandlePostRequest);
        app.Run();
    }

    private static async Task<IResult> HandlePostRequest(HttpRequest request, ILogger<Program> logger)
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
            return Results.Ok();
        }
        catch (System.Text.Json.JsonException e)
        {
            return Results.BadRequest("Failed to parse payload JSON. Reason: " + e.Message);
        }
    }
}
