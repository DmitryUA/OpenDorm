namespace OpenDorm.Application.Abstractions;

public record TokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public interface IJwtTokenGenerator
{
    TokenResult GenerateTokens(
        Guid userId,
        string login,
        IEnumerable<string> roleNames,
        IEnumerable<string> permissions);
}