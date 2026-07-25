using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class PatronymicTests
{
    [Theory]
    [InlineData("Иванович")]
    [InlineData("Александрович")]
    [InlineData("Петрович")]
    [InlineData("Ивановна")]
    [InlineData("Александровна")]
    [InlineData("Петровна")]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var patronymic = new Patronymic(value);
        
        // Assert
        Assert.Equal(value, patronymic.Value);
    }

    [Fact]
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new Patronymic(null!);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new Patronymic(string.Empty);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValueOfMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var veryLongPatronymic = new string('a', Patronymic.MaxLength + 1);
        
        // Act
        var act = () => new Patronymic(veryLongPatronymic);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string veryShortPatronymic = "А";
        
        // Act
        var act = () => new Patronymic(veryShortPatronymic);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData("Ив4нови4")]
    [InlineData("АлЕкСаНдРоВиЧ")]
    [InlineData("18349527")]
    public void Constructor_InvalidValue_ThrowsDomainException(string value)
    {
        // Act
        var act = () => new Patronymic(value);
        
        // Assert
        Assert.Throws<DomainException>(act);
    }
}