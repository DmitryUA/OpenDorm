using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class HouseNumberTests
{
    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("строение 2")]
    [InlineData("47Б")]
    public void Constructor_ValidValue_CreateInstanceAndPreservesValue(string value)
    {
        // Act
        var houseNumber = new HouseNumber(value);
        
        // Assert
        Assert.Equal(value, houseNumber.Value);
    }

    [Fact]
    public void Constructor_ValueOfMaximumLength_CreateInstanceAndPreservesValue()
    {
        // Arrange
        var veryLongNumber = new string('1', HouseNumber.MaxLength);
        
        // Act
        var houseNumber = new HouseNumber(veryLongNumber);
        
        // Assert
        Assert.Equal(veryLongNumber, houseNumber.Value);
    }
    
    [Fact]
    public void Constructor_ValueThatExceedsLengthLimit_ThrowsInvalidHouseNumberException()
    {
        // Arrange
        var veryLongNumber = new string('1', HouseNumber.MaxLength + 1);
        var expectedMessage = $"House number cannot exceed {HouseNumber.MaxLength} characters.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidHouseNumberException>(() => new HouseNumber(veryLongNumber));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_Null_ThrowsInvalidHouseNumberException()
    {
        // Arrange
        const string expectedMessage = "House number cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidHouseNumberException>(() => new HouseNumber(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsInvalidHouseNumberException()
    {
        // Arrange
        const string expectedMessage = "House number cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidHouseNumberException>(() => new HouseNumber(string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }
}