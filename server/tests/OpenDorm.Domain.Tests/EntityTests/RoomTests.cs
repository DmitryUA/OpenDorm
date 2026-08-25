using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.EntityTests;

public class RoomTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidArguments_CreatesRoomWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new RoomName("Room 101");
        var gender = Gender.Male;
        const int capacity = 4;
        const int floor = 2;

        // Act
        var room = new Room(id, name, gender, capacity, floor);

        // Assert
        Assert.Equal(id, room.Id);
        Assert.Equal(name, room.Name);
        Assert.Equal(gender, room.Gender);
        Assert.Equal(capacity, room.Capacity);
        Assert.Equal(floor, room.FloorNumber);
        Assert.True(room.IsActive);
    }
    
    [Fact]
    public void Constructor_DefaultCapacityAndFloor_SetsToOne()
    {
        // Act
        var room = new Room(Guid.NewGuid(), new RoomName("Room 102"), Gender.Female);

        // Assert
        Assert.Equal(1, room.Capacity);
        Assert.Equal(1, room.FloorNumber);
    }
    
    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Room(Guid.NewGuid(), null!, Gender.Male));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidCapacity_ThrowsArgumentOutOfRangeException(int invalidCapacity)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Room(Guid.NewGuid(), new RoomName("Room"), Gender.Male, invalidCapacity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_InvalidFloorNumber_ThrowsArgumentOutOfRangeException(int invalidFloor)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new Room(Guid.NewGuid(), new RoomName("Room"), Gender.Male, 1, invalidFloor));
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_ActiveRoom_IsActiveTrue()
    {
        // Arrange
        var room = new RoomBuilder().Build();
        
        // Act
        room.Activate();
        
        // Assert
        Assert.True(room.IsActive);
    }

    [Fact]
    public void Activate_InactiveRoom_IsActiveTrue()
    {
        // Arrange
        var room = new RoomBuilder().Build();
        room.Deactivate();
        
        // Act
        room.Activate();
        
        // Assert
        Assert.True(room.IsActive);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_ActiveRoom_IsActiveFalse()
    {
        // Arrange
        var room = new RoomBuilder().Build();
        
        // Act
        room.Deactivate();
        
        // Assert
        Assert.False(room.IsActive);
    }

    [Fact]
    public void Deactivate_InactiveRoom_IsActiveFalse()
    {
        // Arrange
        var room = new RoomBuilder().Build();
        room.Deactivate();
        
        // Act
        room.Deactivate();
        
        // Assert
        Assert.False(room.IsActive);
    }

    #endregion
    
    private class RoomBuilder
    {
        public Room Build() => new(Guid.NewGuid(), new RoomName("Room 102"), Gender.Female);
    }
}