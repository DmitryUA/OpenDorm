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
        Assert.True(room.HasVacancy);
        Assert.Empty(room.OccupantIds);
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

    #region CheckIn Tests

    [Fact]
    public void CheckIn_ValidOccupantId_AddsToOccupantsAndUpdatesVacancy()
    {
        // Arrange
        var room = new Room(Guid.NewGuid(), new RoomName("Room 101"), Gender.Male, capacity: 2);
        var occupantId = Guid.NewGuid();

        // Act
        room.CheckIn(occupantId);

        // Assert
        Assert.Contains(occupantId, room.OccupantIds);
        Assert.True(room.HasVacancy);
    }

    [Fact]
    public void CheckIn_RoomIsFull_ThrowsRoomFullyOccupiedException()
    {
        // Arrange
        var room = new Room(Guid.NewGuid(), new RoomName("Room 101"), Gender.Male, capacity: 1);
        room.CheckIn(Guid.NewGuid()); // Заполняем комнату
            
        var newOccupantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<RoomFullyOccupiedException>(() => room.CheckIn(newOccupantId));
        
        Assert.Equal(room.Id, exception.RoomId); 
        Assert.Equal(room.Name, exception.Name);
    }
    
    [Fact]
    public void CheckIn_DuplicateOccupantId_ThrowsDuplicateOccupantException()
    {
        // Arrange
        var room = new Room(Guid.NewGuid(), new RoomName("Room 101"), Gender.Male, capacity: 2);
        var occupantId = Guid.NewGuid();
        room.CheckIn(occupantId);

        // Act & Assert
        var exception = Assert.Throws<DuplicateOccupantException>(() => room.CheckIn(occupantId));
        
        Assert.Equal(occupantId, exception.OccupantId);
    }
    
    #endregion

    #region CheckOutTests

    [Fact]
    public void CheckOut_ExistingOccupantId_RemovesFromOccupantsAndUpdatesVacancy()
    {
        // Arrange
        var room = new Room(Guid.NewGuid(), new RoomName("Room 101"), Gender.Male, capacity: 1);
        var occupantId = Guid.NewGuid();
        room.CheckIn(occupantId);
        Assert.False(room.HasVacancy); // Комната заполнена

        // Act
        room.CheckOut(occupantId);

        // Assert
        Assert.DoesNotContain(occupantId, room.OccupantIds);
    }

    [Fact]
    public void CheckOut_NonExistingOccupantId_ThrowsDomainException()
    {
        // Arrange
        var room = new Room(Guid.NewGuid(), new RoomName("Room 101"), Gender.Male);
        var nonExistingOccupantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => room.CheckOut(nonExistingOccupantId));
        Assert.Contains("Occupant is not in this room", exception.Message);
    }

    #endregion
}