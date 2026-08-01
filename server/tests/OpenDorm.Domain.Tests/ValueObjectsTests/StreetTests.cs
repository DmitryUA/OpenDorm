using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class StreetTests
{
    [Fact]
    public void Ctor_ValidValue_CreatesInstanceAndPreservesValue()
    {
        // Arrange
        const string name = "Улица Ленина";

        // Act
        var street = new Street(name);

        // Assert
        Assert.Equal(name, street.Value);
    }

    [Fact]
    public void Ctor_LeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string rawName = "  Улица Ленина  ";
        const string expectedName = "Улица Ленина";

        // Act
        var street = new Street(rawName);

        // Assert
        Assert.Equal(expectedName, street.Value);
        Assert.NotEqual(rawName, street.Value);
    }
    
    [Fact]
    public void Ctor_MinValidLength_CreatesInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('X', Street.MinStreetNameLength);
        
        // Act
        var street = new Street(minValidName);

        // Assert
        Assert.Equal(minValidName, street.Value);
    }

    [Fact]
    public void Ctor_MaxValidLength_CreatesInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('X', Street.MaxStreetNameLength);

        // Act
        var street = new Street(maxValidName);

        // Assert
        Assert.Equal(maxValidName, street.Value);
    }

    [Fact]
    public void Ctor_Null_ThrowsInvalidStreetException()
    {
        // Arrange
        const string expectedMessage = "Street name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidStreetException>(() => new Street(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_EmptyOrWhitespace_ThrowsInvalidStreetException(string name)
    {
        // Arrange
        const string expectedMessage = "Street name cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidStreetException>(() => new Street(name));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public void Ctor_LengthLessThanMin_ThrowsInvalidStreetException(string name)
    {
        // Arrange
        var expectedMessage = $"Street name cannot be less than {Street.MinStreetNameLength} characters long.";

        // Act & Assert
        var exception = Assert.Throws<InvalidStreetException>(() => new Street(name));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Ctor_LengthGreaterThanMax_ThrowsInvalidStreetException()
    {
        // Arrange
        var longName = new string('A', Street.MaxStreetNameLength + 1);
        var expectedMessage = $"Street name cannot exceed {Street.MaxStreetNameLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidStreetException>(() => new Street(longName));
        Assert.Equal(expectedMessage, exception.Message);
    }
}