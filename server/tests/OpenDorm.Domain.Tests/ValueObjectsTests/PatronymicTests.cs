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
    public void Constructor_Null_ThrowsInvalidPatronymicException()
    {
        // Arrange
        const string expectedMessage = "Patronymic cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidPatronymicException>(() => new Patronymic(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidPatronymicException()
    {
        // Arrange
        const string expectedMessage = "Patronymic cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidPatronymicException>(() => new Patronymic(string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueExceedsMaximumLength_ThrowsInvalidPatronymicException()
    {
        // Arrange
        var veryLongPatronymic = new string('a', Patronymic.MaxLength + 1);
        var expectedMessage = $"Patronymic cannot be exceed {Patronymic.MaxLength} characters.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidPatronymicException>(() => new Patronymic(veryLongPatronymic));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueShorterThanMinimum_ThrowsInvalidPatronymicException()
    {
        // Arrange
        const string veryShortPatronymic = "А";
        var expectedMessage = $"Patronymic cannot be less than {Patronymic.MinLength} characters long.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidPatronymicException>(() => new Patronymic(veryShortPatronymic));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("Ив4нови4")]
    [InlineData("АлЕкСаНдРоВиЧ")]
    [InlineData("18349527")]
    public void Constructor_InvalidValue_ThrowsInvalidPatronymicException(string value)
    {
        // Arrange
        const string expectedMessage =
            "Patronymic must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidPatronymicException>(() => new Patronymic(value));
        Assert.Equal(expectedMessage, exception.Message);
    }
}