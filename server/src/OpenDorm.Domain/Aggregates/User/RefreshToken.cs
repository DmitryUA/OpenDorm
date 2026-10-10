using OpenDorm.Domain.Common;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Aggregates.User;

public class RefreshToken : Entity
{
    public const int MaxLength = 255;
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private RefreshToken() { } // For EF Core only

    public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt) : base(id)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidRefreshTokenException("Token cannot be empty.");
        if (token.Length > MaxLength)
            throw new InvalidRefreshTokenException($"Token cannot exceed {MaxLength} characters.");
            
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke() => IsRevoked = true;

    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}