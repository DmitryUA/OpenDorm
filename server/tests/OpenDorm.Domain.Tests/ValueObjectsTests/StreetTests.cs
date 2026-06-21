using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class StreetTests
{
    [Fact]
    public void Ctor_WithValidName_CreatesInstanceAndPreservesName()
    {
        // Arrange
        const string name = "Улица Ленина";

        // Act
        var street = new Street(name);

        // Assert
        Assert.Equal(name, street.Name);
    }

    [Fact]
    public void Ctor_WithLeadingAndTrailingWhitespace_TrimsName()
    {
        // Arrange
        const string rawName = "  Улица Ленина  ";
        const string expectedName = "Улица Ленина";

        // Act
        var street = new Street(rawName);

        // Assert
        Assert.Equal(expectedName, street.Name);
        Assert.NotEqual(rawName, street.Name);
    }

    [Fact]
    public void Ctor_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        const string? nullName = null;

        // Act
        var act = () => new Street(nullName!);

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_WithEmptyOrWhitespace_ThrowsArgumentException(string name)
    {
        // Arrange & Act
        var act = () => new Street(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public void Ctor_WithLengthLessThanMin_ThrowsArgumentOutOfRangeException(string name)
    {
        // Arrange & Act
        var act = () => new Street(name);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Ctor_WithLengthGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var longName = new string('A', Street.MaxStreetNameLength + 1);

        // Act
        var act = () => new Street(longName);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Ctor_WithMinValidLength_CreatesInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('X', Street.MinStreetNameLength);
        
        // Act
        var street = new Street(minValidName);

        // Assert
        Assert.Equal(minValidName, street.Name);
    }

    [Fact]
    public void Ctor_WithMaxValidLength_CreatesInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('X', Street.MaxStreetNameLength);

        // Act
        var street = new Street(maxValidName);

        // Assert
        Assert.Equal(maxValidName, street.Name);
    }
}