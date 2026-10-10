using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class LoginTests
{
    [Theory]
    [InlineData("admin")]
    [InlineData("ivanov1984")]
    [InlineData("admin-ivanov")]
    [InlineData("..admin-ivanov-1984.")]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var login = new Login(value);
        
        // Assert
        Assert.Equal(value, login.Value);
    }

    [Fact]
    public void Constructor_Null_ThrowsInvalidLoginException()
    {
        // Arrange
        const string expectedMessage = "Login cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLoginException>(() => new Login(null!));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidLoginException()
    {
        // Arrange
        const string expectedMessage = "Login cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLoginException>(() => new Login(string.Empty));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_OverlyLongLogin_ThrowsInvalidLoginException()
    {
        // Arrange
        var overlyLongLogin = new string('a', Login.MaxLength + 1);
        var expectedMessage = $"Login length must be between {Login.MinLength} and {Login.MaxLength} characters. " +
                              $"Actual: {overlyLongLogin.Length}.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLoginException>(() => new Login(overlyLongLogin));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_VeryShortLogin_ThrowsInvalidLoginException()
    {
        // Arrange
        var veryShortLogin = new string('a', Login.MinLength - 1);
        var expectedMessage = $"Login length must be between {Login.MinLength} and {Login.MaxLength} characters. " +
                              $"Actual: {veryShortLogin.Length}.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLoginException>(() => new Login(veryShortLogin));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("логин")]
    [InlineData("admin!")]
    [InlineData("ivanov}ivan")]
    [InlineData("admin ivanov")]
    public void Constructor_InvalidPattern_ThrowsInvalidLoginException(string value)
    {
        // Arrange
        const string expectedMessage = "Login can contain only lowercase letters (a-z), " +
                                       "digits (0-9), and symbols '_', '-', '.'.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLoginException>(() => new Login(value));
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}