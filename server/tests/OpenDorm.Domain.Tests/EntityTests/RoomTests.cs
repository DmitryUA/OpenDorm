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
        var capacity = new RoomCapacity(4);
        var floor = new FloorNumber(2);

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
    public void Constructor_NullName_ThrowsInvalidRoomException()
    {
        // Act & Assert
        Assert.Throws<InvalidRoomException>(() => 
            new Room(Guid.NewGuid(), null!, Gender.Male, new RoomCapacity(1), new FloorNumber(1)));
    }
    
    [Fact]
    public void Constructor_NullCapacity_ThrowsInvalidRoomException()
    {
        // Act & Assert
        Assert.Throws<InvalidRoomException>(() => 
            new Room(Guid.NewGuid(), new RoomName("406"), Gender.Male, null!, new FloorNumber(1)));
    }
    
    [Fact]
    public void Constructor_NullFloorNumber_ThrowsInvalidRoomException()
    {
        // Act & Assert
        Assert.Throws<InvalidRoomException>(() => 
            new Room(Guid.NewGuid(), new RoomName("406"), Gender.Male, new RoomCapacity(1), null!));
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_ActiveRoom_IsActiveTrue()
    {
        // Arrange
        var room = RoomBuilder.Build();
        
        // Act
        room.Activate();
        
        // Assert
        Assert.True(room.IsActive);
    }

    [Fact]
    public void Activate_InactiveRoom_IsActiveTrue()
    {
        // Arrange
        var room = RoomBuilder.Build();
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
        var room = RoomBuilder.Build();
        
        // Act
        room.Deactivate();
        
        // Assert
        Assert.False(room.IsActive);
    }

    [Fact]
    public void Deactivate_InactiveRoom_IsActiveFalse()
    {
        // Arrange
        var room = RoomBuilder.Build();
        room.Deactivate();
        
        // Act
        room.Deactivate();
        
        // Assert
        Assert.False(room.IsActive);
    }

    #endregion
    
    private static class RoomBuilder
    {
        public static Room Build() => new(
            Guid.NewGuid(),
            new RoomName("Room 102"),
            Gender.Female, new RoomCapacity(1),
            new FloorNumber(1));
    }
}