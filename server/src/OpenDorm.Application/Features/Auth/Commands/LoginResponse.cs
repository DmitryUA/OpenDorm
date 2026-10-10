namespace OpenDorm.Application.Features.Auth.Commands;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);