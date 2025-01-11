using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Telegram.Bot;

namespace TunnelGPT.Utils;

public static class AppCompositionRoot
{
    public static IUpdateProcessor CreateUpdateProcessor()
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddSingleton<ITelegramBotClient>(provider => 
            new TelegramBotClient(EnvironmentUtils.GetTelegramBotToken()));
        serviceCollection.AddSingleton<ChatClient>(provider => 
            new ChatClient("gpt-4o", EnvironmentUtils.GetOpenAiApiKey()));
        serviceCollection.AddScoped<IUpdateProcessor, UpdateProcessor>();
        
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IUpdateProcessor>();
    }
}