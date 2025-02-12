using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Telegram.Bot.Types;
using TunnelGPT.Infrastructure;

namespace TunnelGPT;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        WebApplication app = builder.Build();

        app.MapGet("/", () => "TunnelGPT is running!");
        app.MapPost("/", HandlePostRequest);
        app.Run();
    }

    private static async Task<IResult> HandlePostRequest(HttpRequest request)
    {
        if (!ValidateTelegramBotSecret(request))
        {
            return Results.Unauthorized();
        }
        
        if (request.ContentLength is null or 0)
        {
            return Results.BadRequest("Request body must not be empty.");
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

    private static bool ValidateTelegramBotSecret(HttpRequest request)
    {
        string expectedSecret = EnvironmentUtils.GetTelegramBotSecret();
        string? secret = request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
        return secret is not null && secret == expectedSecret;
    }
}
