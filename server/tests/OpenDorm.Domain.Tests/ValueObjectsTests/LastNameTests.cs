using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class LastNameTests
{
    [Theory]
    [InlineData("Иванов")]
    [InlineData("Петров")]
    [InlineData("Сидоров")]
    [InlineData("Иванова")]
    [InlineData("Петрова")]
    [InlineData("Сидорова")]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var lastName = new LastName(value);
        
        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void Constructor_Null_ThrowsInvalidLastNameException()
    {
        // Arrange
        const string expectedMessage = "Last name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLastNameException>(() => new LastName(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidLastNameException()
    {
        // Arrange
        const string expectedMessage = "Last name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLastNameException>(() => new LastName(string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueExceedsMaximumLength_ThrowsInvalidLastNameException()
    {
        // Arrange
        var veryLongLastName = new string('a', LastName.MaxLength + 1);
        var expectedMessage = $"Last name cannot be exceed {LastName.MaxLength} characters.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLastNameException>(() => new LastName(veryLongLastName));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsInvalidLastNameException()
    {
        // Arrange
        const string veryShortLastName = "А";
        var expectedMessage = $"Last name cannot be less than {LastName.MinLength} characters long.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLastNameException>(() => new LastName(veryShortLastName));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("Дми3т1рий")]
    [InlineData("ВиКтоРиЯ")]
    [InlineData("18349527")]
    public void Constructor_InvalidValue_ThrowsInvalidLastNameException(string value)
    {
        // Arrange
        const string expectedMessage =
            "Last name must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidLastNameException>(()=>new LastName(value));
        Assert.Equal(expectedMessage, exception.Message);
    }
}