using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class FloorNumberTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(25)]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue(int value)
    {
        // Act
        var floorNumber = new FloorNumber(value);
        
        // Assert
        Assert.Equal(value, floorNumber.Value);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidArguments_ThrowsInvalidFloorNumberException(int value)
    {
        // Arrange
        const string expectedMessage = "Floor number cannot be less than 1.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidFloorNumberException>(() => new FloorNumber(value));
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}