using System.Text.Json;
using TunnelGPT.Infrastructure.Configuration;
using TunnelGPT.Infrastructure.Responses;

namespace TunnelGPT.Infrastructure.Middleware;

public class TelegramWebhookValidation(
    RequestDelegate next, 
    AppSettings appSettings, 
    ILogger<TelegramWebhookValidation> logger
    )
{
    private const string TelegramBotSecretHeader = "X-Telegram-Bot-Api-Secret-Token";
    private readonly string _expectedSecret = appSettings.TelegramBotSecret;

    public async Task Invoke(HttpContext context)
    {
        string? secret = context.Request.Headers[TelegramBotSecretHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(secret) || secret != _expectedSecret)
        {
            logger.LogWarning("Unauthorized request: Incorrect {HeaderName} header.", TelegramBotSecretHeader);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            ErrorResponse errorResponse = new("Unauthorized", $"Incorrect {TelegramBotSecretHeader} header.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }
        
        await next(context);
    }
}