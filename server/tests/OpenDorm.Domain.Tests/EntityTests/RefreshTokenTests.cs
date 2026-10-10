using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Tests.EntityTests;

public class RefreshTokenTests
{
    #region Constructor

    [Fact]
    public void Constructor_ValidArguments_CreatesInstanceAndPreservesValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string token = "refresh_token_abc123";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(id, userId, token, expiresAt);

        // Assert
        Assert.Equal(id, refreshToken.Id);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsCreatedAtCloseToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(refreshToken.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsIsRevokedToFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);

        // Assert
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public void Constructor_TokenExactlyMaxLength_CreatesInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var maxLengthToken = new string('a', RefreshToken.MaxLength);

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, maxLengthToken, expiresAt);

        // Assert
        Assert.Equal(maxLengthToken, refreshToken.Token);
    }

    [Fact]
    public void Constructor_NullToken_ThrowsInvalidRefreshTokenException()
    {
        // Arrange
        const string expectedMessage = "Token cannot be empty.";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<InvalidRefreshTokenException>(() =>
            new RefreshToken(Guid.NewGuid(), userId, null!, expiresAt));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyToken_ThrowsInvalidRefreshTokenException()
    {
        // Arrange
        const string expectedMessage = "Token cannot be empty.";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<InvalidRefreshTokenException>(() =>
            new RefreshToken(Guid.NewGuid(), userId, string.Empty, expiresAt));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceToken_ThrowsInvalidRefreshTokenException()
    {
        // Arrange
        const string expectedMessage = "Token cannot be empty.";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<InvalidRefreshTokenException>(() =>
            new RefreshToken(Guid.NewGuid(), userId, "   ", expiresAt));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_TokenExceedsMaxLength_ThrowsInvalidRefreshTokenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var overlyLongToken = new string('a', RefreshToken.MaxLength + 1);
        var expectedMessage = $"Token cannot exceed {RefreshToken.MaxLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidRefreshTokenException>(() =>
            new RefreshToken(Guid.NewGuid(), userId, overlyLongToken, expiresAt));
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region Revoke

    [Fact]
    public void Revoke_NotRevokedToken_SetsIsRevokedToTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);

        // Act
        refreshToken.Revoke();

        // Assert
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public void Revoke_AlreadyRevokedToken_DoesNotThrowAndKeepsIsRevokedTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);
        refreshToken.Revoke();

        // Act
        refreshToken.Revoke();

        // Assert
        Assert.True(refreshToken.IsRevoked);
    }

    #endregion

    #region IsValid

    [Fact]
    public void IsValid_NotRevokedAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);

        // Act & Assert
        Assert.True(refreshToken.IsValid);
    }

    [Fact]
    public void IsValid_NotRevokedButExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(-1);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);

        // Act & Assert
        Assert.False(refreshToken.IsValid);
    }

    [Fact]
    public void IsValid_RevokedButNotExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);
        refreshToken.Revoke();

        // Act & Assert
        Assert.False(refreshToken.IsValid);
    }

    [Fact]
    public void IsValid_RevokedAndExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(-1);
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, "token", expiresAt);
        refreshToken.Revoke();

        // Act & Assert
        Assert.False(refreshToken.IsValid);
    }

    #endregion
}