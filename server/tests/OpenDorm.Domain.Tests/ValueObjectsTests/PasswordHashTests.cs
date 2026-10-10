using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class PasswordHashTests
{
    [Theory]
    [InlineData("hash")]
    [InlineData("$2a$12$BCryptHashValue123!@#")]
    [InlineData("a")]
    [InlineData("hash with spaces inside")]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var passwordHash = new PasswordHash(value);

        // Assert
        Assert.Equal(value, passwordHash.Value);
    }

    [Fact]
    public void Constructor_ExactlyMaxLength_CreateInstanceAndPreservesValue()
    {
        // Arrange
        var maxLengthHash = new string('a', PasswordHash.MaxLength);

        // Act
        var passwordHash = new PasswordHash(maxLengthHash);

        // Assert
        Assert.Equal(maxLengthHash, passwordHash.Value);
    }

    [Fact]
    public void Constructor_PreservesValueWithoutTrimming()
    {
        // Arrange
        const string valueWithWhitespace = "  hash_with_leading_and_trailing_spaces  ";

        // Act
        var passwordHash = new PasswordHash(valueWithWhitespace);

        // Assert
        Assert.Equal(valueWithWhitespace, passwordHash.Value);
    }

    [Fact]
    public void Constructor_Null_ThrowsInvalidPasswordHashException()
    {
        // Arrange
        const string expectedMessage = "Password hash cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidPasswordHashException>(() => new PasswordHash(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidPasswordHashException()
    {
        // Arrange
        const string expectedMessage = "Password hash cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidPasswordHashException>(() => new PasswordHash(string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceString_ThrowsInvalidPasswordHashException()
    {
        // Arrange
        const string expectedMessage = "Password hash cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidPasswordHashException>(() => new PasswordHash("   "));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_OverlyLongPasswordHash_ThrowsInvalidPasswordHashException()
    {
        // Arrange
        var overlyLongHash = new string('a', PasswordHash.MaxLength + 1);
        var expectedMessage = $"Password hash cannot exceed {PasswordHash.MaxLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidPasswordHashException>(() => new PasswordHash(overlyLongHash));
        Assert.Equal(expectedMessage, exception.Message);
    }
}