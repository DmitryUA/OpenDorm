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
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new FirstName(null!);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new FirstName(string.Empty);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValueOfMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var veryLongFirstName = new string('a', FirstName.MaxLength + 1);
        
        // Act
        var act = () => new FirstName(veryLongFirstName);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string veryShortFirstName = "А";
        
        // Act
        var act = () => new FirstName(veryShortFirstName);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData("Дми3т1рий")]
    [InlineData("ВиКтоРиЯ")]
    [InlineData("18349527")]
    public void Constructor_InvalidValue_ThrowsDomainException(string value)
    {
        // Act
        var act = () => new FirstName(value);
        
        // Assert
        Assert.Throws<DomainException>(act);
    }
}