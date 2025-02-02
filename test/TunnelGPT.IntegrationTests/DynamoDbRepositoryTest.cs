using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using TunnelGPT.Core.DataModel;
using TunnelGPT.Infrastructure;
using TunnelGPT.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace TunnelGPT.IntegrationTests;

public class DynamoDbRepositoryTest : IClassFixture<DynamoDbFixture>
{
    private const string DynamoDbLocalUrl = "http://localhost:8000";
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDbRepository _dynamoDbRepository;
    private readonly ILambdaLogger _logger;
    private readonly ITestOutputHelper _output;

    public DynamoDbRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
        _logger = new TestLambdaContext().Logger;
        _dynamoDbClient = DependencyFactory.CreateDynamoDbClient(DynamoDbLocalUrl);
        IDynamoDBContext dynamoDbContext = DependencyFactory.CreateDynamoDbContext(_dynamoDbClient);
        _dynamoDbRepository = new(dynamoDbContext,_logger);
    }

    [Fact]
    public async Task SaveUserAsync_ShouldSaveUserToDatabase_WhenValidInputIsProvided()
    {
        // Arrange
        User userToWriteToDb = new() { UserId = 1, Username = "alice", };
        await InitializeDatabaseAsync();
        
        // Act
        await _dynamoDbRepository.SaveUserAsync(userToWriteToDb.UserId, userToWriteToDb.Username);
        User? userExtractedFromDb = await _dynamoDbRepository.GetUserByIdAsync(userToWriteToDb.UserId);
        
        // Assert
        Assert.Equal(userToWriteToDb, userExtractedFromDb);
    }
    
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserNotFoundInDatabase()
    {
        // Arrange
        await InitializeDatabaseAsync();
        User userToReadFromDb = new() { UserId = 404, Username = "otto", };
        
        // Act
        User? userExtractedFromDb = await _dynamoDbRepository.GetUserByIdAsync(userToReadFromDb.UserId);
        
        // Assert
        Assert.Null(userExtractedFromDb);
    }

    private async Task InitializeDatabaseAsync()
    {
        const string tableName = "tunnelgpt";
        const string partitionKeyName = "user_id";
        CreateTableRequest tableRequest = new()
        {
            TableName = tableName,
            AttributeDefinitions = [new AttributeDefinition { AttributeName = partitionKeyName, AttributeType = "N" }],
            KeySchema = [new KeySchemaElement { AttributeName = partitionKeyName, KeyType = "HASH" }],
            ProvisionedThroughput = new ProvisionedThroughput(1, 1)
        };

        ListTablesResponse? tables = await _dynamoDbClient.ListTablesAsync();
        if (!tables.TableNames.Contains(tableName))
        {
            await _dynamoDbClient.CreateTableAsync(tableRequest);
            _output.WriteLine($"Created table '{tableName}'.");
        }
        else
        {
            _output.WriteLine($"Table '{tableName}' already exists.");
        }
    }
}