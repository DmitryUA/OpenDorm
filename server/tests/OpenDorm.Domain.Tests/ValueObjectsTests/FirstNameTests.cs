using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class FirstNameTests
{
    [Theory]
    [InlineData("Дмитрий")]
    [InlineData("Александр")]
    [InlineData("Игорь")]
    [InlineData("Виктория")]
    [InlineData("Вера")]
    [InlineData("Анастасия")]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var firstName = new FirstName(value);
        
        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void Constructor_Null_ThrowsInvalidFirstNameException()
    {
        // Arrange
        const string expectedMessage = "First name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFirstNameException>(() => new FirstName(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidFirstNameException()
    {
        // Arrange
        const string expectedMessage = "First name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFirstNameException>(() => new FirstName(string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueExceedsMaximumLength_ThrowsInvalidFirstNameException()
    {
        // Arrange
        var veryLongFirstName = new string('a', FirstName.MaxLength + 1);
        var expectedMessage = $"First name cannot be exceed {FirstName.MaxLength} characters.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFirstNameException>(() => new FirstName(veryLongFirstName));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsInvalidFirstNameException()
    {
        // Arrange
        const string veryShortFirstName = "А";
        var expectedMessage = $"First name cannot be less than {FirstName.MinLength} characters long.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFirstNameException>(() => new FirstName(veryShortFirstName));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("Дми3т1рий")]
    [InlineData("ВиКтоРиЯ")]
    [InlineData("18349527")]
    public void Constructor_InvalidValue_ThrowsInvalidFirstNameException(string value)
    {
        // Arrange
        const string expectedMessage =
            "First name must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFirstNameException>(() => new FirstName(value));
        Assert.Equal(expectedMessage, exception.Message);
    }
}