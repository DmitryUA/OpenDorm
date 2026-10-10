using OpenDorm.Infrastructure.Services;

namespace OpenDorm.Infrastructure.Tests.ServicesTests;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    #region Hash

    [Fact]
    public void Hash_ValidPassword_ReturnsPasswordHash()
    {
        // Arrange
        const string plainPassword = "SecurePassword123!";

        // Act
        var result = _sut.Hash(plainPassword);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(plainPassword, result.Value);
    }

    [Fact]
    public void Hash_ValidPassword_ReturnsBcryptFormat()
    {
        // Arrange
        const string plainPassword = "SecurePassword123!";

        // Act
        var result = _sut.Hash(plainPassword);

        // Assert
        Assert.StartsWith("$2", result.Value);
    }

    [Fact]
    public void Hash_SamePasswordMultipleTimes_ReturnsDifferentHashes()
    {
        // Arrange
        const string plainPassword = "SecurePassword123!";

        // Act
        var hash1 = _sut.Hash(plainPassword);
        var hash2 = _sut.Hash(plainPassword);

        // Assert
        Assert.NotEqual(hash1.Value, hash2.Value);
    }

    [Fact]
    public void Hash_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        const string expectedParamName = "plainPassword";
        const string expectedMessage = "Password cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Hash(null!));
        Assert.Equal(expectedParamName, exception.ParamName);
        Assert.StartsWith(expectedMessage, exception.Message);
    }

    [Fact]
    public void Hash_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        const string expectedParamName = "plainPassword";
        const string expectedMessage = "Password cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Hash(string.Empty));
        Assert.Equal(expectedParamName, exception.ParamName);
        Assert.StartsWith(expectedMessage, exception.Message);
    }

    [Fact]
    public void Hash_WhitespacePassword_ThrowsArgumentException()
    {
        // Arrange
        const string expectedParamName = "plainPassword";
        const string expectedMessage = "Password cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _sut.Hash("   "));
        Assert.Equal(expectedParamName, exception.ParamName);
        Assert.StartsWith(expectedMessage, exception.Message);
    }

    #endregion

    #region Verify

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string plainPassword = "SecurePassword123!";
        var hash = _sut.Hash(plainPassword);

        // Act
        var result = _sut.Verify(plainPassword, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string correctPassword = "SecurePassword123!";
        const string incorrectPassword = "WrongPassword456!";
        var hash = _sut.Hash(correctPassword);

        // Act
        var result = _sut.Verify(incorrectPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_CaseSensitive_ReturnsFalseForDifferentCase()
    {
        // Arrange
        const string originalPassword = "Secure-Password123!";
        const string differentCasePassword = "secure-password123!";
        var hash = _sut.Hash(originalPassword);

        // Act
        var result = _sut.Verify(differentCasePassword, hash);

        // Assert
        Assert.False(result);
    }

    #endregion
}