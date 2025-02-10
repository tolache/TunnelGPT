namespace TunnelGPT.Infrastructure;

public static class EnvironmentUtils
{
    public static string GetOpenAiApiKey()
    {
        return GetEnvironmentVariableOrThrow("OPENAI_API_KEY");
    }
    
    public static string GetTelegramBotToken()
    {
        return GetEnvironmentVariableOrThrow("TELEGRAM_BOT_TOKEN");
    }

    private static string GetEnvironmentVariableOrThrow(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Environment variable {variableName} is not set.");
        }
        return value;
    }
}