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
    private const string TelegramWebhookSecretHeader = "X-Telegram-Bot-Api-Secret-Token";
    private readonly string _expectedSecret = appSettings.TelegramWebhookSecret;

    public async Task Invoke(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await next(context);
            return;
        }
        string? secret = context.Request.Headers[TelegramWebhookSecretHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(secret) || secret != _expectedSecret)
        {
            logger.LogWarning("Unauthorized request: Incorrect {HeaderName} header.", TelegramWebhookSecretHeader);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            ErrorResponse errorResponse = new("Unauthorized", $"Incorrect {TelegramWebhookSecretHeader} header.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }
        
        await next(context);
    }
}