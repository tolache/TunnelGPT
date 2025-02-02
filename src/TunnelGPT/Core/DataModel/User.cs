using Amazon.DynamoDBv2.DataModel;

namespace TunnelGPT.Core.DataModel;

[DynamoDBTable("tunnelgpt")]
public class User : IEquatable<User>
{
    [DynamoDBHashKey("user_id")]
    public required long UserId { get; init; }
    
    [DynamoDBProperty("username")]
    public string? Username { get; init; }

    public bool Equals(User? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserId == other.UserId && Username == other.Username;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((User)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserId, Username);
    }

    public static bool operator ==(User? left, User? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(User? left, User? right) => !(left == right);
}