namespace TunnelGPT.Infrastructure.Configuration;

public class AppSettings
{
    public required string OpenAiApiKey { get; init; }
    public required string TelegramBotSecret { get; init; }
    public required string TelegramBotToken { get; init; }

    public static AppSettings LoadFromConfiguration(IConfiguration configuration)
    {
        return new AppSettings
        {
            OpenAiApiKey = GetRequiredSetting(configuration, "OPENAI_API_KEY"),
            TelegramBotSecret = GetRequiredSetting(configuration, "TELEGRAM_BOT_SECRET"),
            TelegramBotToken = GetRequiredSetting(configuration, "TELEGRAM_BOT_TOKEN"),
        };
    }

    private static string GetRequiredSetting(IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Environment variable {key} is not set.");
        }

        return value;
    }
}