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

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        try
        {
            User user = await dynamoDbContext.LoadAsync<User>(userId);
            if (user is null)
            {
                logger.LogDebug($"User with id wasn't found in the database.");
                return null;
            }
            logger.LogDebug($"Found user '{user.Username}' with id '{user.UserId}' in the database.");
            return user;
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to load user from the database. Reason: {e.Message}");
            logger.LogDebug(e.StackTrace);
            throw;
        }
    }
}