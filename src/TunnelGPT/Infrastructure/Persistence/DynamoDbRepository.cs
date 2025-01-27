using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using TunnelGPT.Core.DataModel;

namespace TunnelGPT.Infrastructure.Persistence;

public class DynamoDbRepository(IDynamoDBContext dynamoDbContext, ILambdaLogger logger)
{
    public async Task SaveUserAsync(long userId, string username)
    {
        User tunnelGptUser = new()
        {
            UserId = userId,
            Username = username,
        };
        try
        {
            await dynamoDbContext.SaveAsync(tunnelGptUser);
            logger.LogInformation($"Saved user '{username}' with id '{userId}' to the database.");
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to write user to the database. Reason: {e.Message}");
            logger.LogDebug(e.StackTrace);
            throw;
        }
    }
}