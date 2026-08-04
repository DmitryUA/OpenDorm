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
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LastName(null!);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LastName(string.Empty);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValueOfMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var veryLongLastName = new string('a', LastName.MaxLength + 1);
        
        // Act
        var act = () => new LastName(veryLongLastName);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string veryShortLastName = "А";
        
        // Act
        var act = () => new LastName(veryShortLastName);
        
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
        var act = () => new LastName(value);
        
        // Assert
        Assert.Throws<DomainException>(act);
    }
}