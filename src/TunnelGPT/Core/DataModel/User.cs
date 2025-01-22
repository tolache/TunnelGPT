using Amazon.DynamoDBv2.DataModel;

namespace TunnelGPT.Core.DataModel;

[DynamoDBTable("tunnelgpt")]
public class User
{
    [DynamoDBHashKey("user_id")]
    public required long UserId { get; set; }
    
    [DynamoDBProperty("username")]
    public string? Username { get; set; }
}