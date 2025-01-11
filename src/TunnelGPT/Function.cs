using Amazon.Lambda.Core;
using Telegram.Bot.Types;
using TunnelGPT.Utils;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TunnelGPT;

public class Function
{
    private readonly IUpdateProcessor _updateProcessor;

    public Function() : this(AppCompositionRoot.CreateUpdateProcessor()) {}

    private Function(IUpdateProcessor updateProcessor)
    {
        _updateProcessor = updateProcessor;
    }
    
    /// <summary>
    /// A simple function that queries Telegram API
    /// </summary>
    /// <param name="update">A Telegram update object.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public Response FunctionHandler(Update update, ILambdaContext context)
    {
            _updateProcessor.ProcessUpdateAsync(update);
            return new Response("Success", "Update processed");
    }
    
    public record Response(string Status, string Message);
}
