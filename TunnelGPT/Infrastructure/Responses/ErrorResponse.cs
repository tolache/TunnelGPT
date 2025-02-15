namespace TunnelGPT.Infrastructure.Responses;

public class ErrorResponse(string error, string message)
{
    public string Error { get; init; } = error;
    public string Message { get; init; } = message;
}