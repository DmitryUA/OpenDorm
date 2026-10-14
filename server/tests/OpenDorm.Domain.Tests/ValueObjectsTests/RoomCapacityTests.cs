using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class RoomCapacityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(10)]
    public void Constructor_ValidArguments_CreatesInstanceAndPreservesValue(int value)
    {
        // Act
        var capacity = new RoomCapacity(value);
        
        // Assert
        Assert.Equal(value, capacity.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Constructor_InvalidArguments_ThrowsInvalidRoomCapacityException(int value)
    {
        // Arrange
        const string expectedMessage = "Room capacity cannot be less than 1.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidRoomCapacityException>(() => new RoomCapacity(value));
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}