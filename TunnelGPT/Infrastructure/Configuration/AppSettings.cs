namespace TunnelGPT.Infrastructure.Configuration;

public class AppSettings
{
    public required string OpenAiApiKey { get; init; }
    public required string OpenAiModel { get; init; }
    public required string TelegramWebhookSecret { get; init; }
    public required string TelegramBotToken { get; init; }

    public static AppSettings LoadFromConfiguration(IConfiguration configuration)
    {
        return new AppSettings
        {
            OpenAiApiKey = GetRequiredSetting(configuration, "OPENAI_API_KEY"),
            OpenAiModel = GetRequiredSetting(configuration, "OPENAI_MODEL"),
            TelegramBotToken = GetRequiredSetting(configuration, "TELEGRAM_BOT_TOKEN"),
            TelegramWebhookSecret = GetRequiredSetting(configuration, "TELEGRAM_WEBHOOK_SECRET"),
        };
    }

    private static string GetRequiredSetting(IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Required configuration setting '{key}' is missing or empty. " +
                                                "Set it via environment variables or other configuration providers. " +
                                                "Learn more at https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers");
        }

        return value;
    }
}