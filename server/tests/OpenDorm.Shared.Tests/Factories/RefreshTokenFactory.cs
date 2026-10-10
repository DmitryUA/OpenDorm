using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Shared.Tests.Factories;

public static class RefreshTokenFactory
{
    public const string DefaultToken = "refresh_token_abc123";

    /// <summary>
    /// Создаёт валидный refresh-токен с указанными параметрами.
    /// По умолчанию токен действителен 7 дней с текущего момента.
    /// </summary>
    public static RefreshToken Create(
        Guid? userId = null,
        string? token = null,
        DateTime? expiresAt = null)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            token ?? DefaultToken,
            expiresAt ?? DateTime.UtcNow.AddDays(7));
    }

    /// <summary>
    /// Создаёт истёкший refresh-токен (срок действия — вчера).
    /// </summary>
    public static RefreshToken CreateExpired(
        Guid? userId = null,
        string? token = null)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            token ?? DefaultToken,
            DateTime.UtcNow.AddDays(-1));
    }
}