using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Domain.Tests.AggregateTests;

public class DormitoryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidArguments_CreateDormitoryWithValidProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = new Address(new City("Москва"), new Street("Советская"), new HouseNumber("41"));
        const int floorCount = 9;
        
        // Act
        var dormitory = new Dormitory(id, address, floorCount);
        
        // Assert
        Assert.Equal(id, dormitory.Id);
        Assert.Equal(address, dormitory.Address);
        Assert.Equal(floorCount, dormitory.FloorCount);
    }

    [Fact]
    public void Constructor_DefaultFloorCount_FloorCountSetToOne()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = new Address(new City("Москва"), new Street("Советская"), new HouseNumber("41"));
        
        // Act
        var dormitory = new Dormitory(id, address);

        // Assert
        Assert.Equal(1, dormitory.FloorCount);
    }

    [Fact]
    public void Constructor_NullAddress_ThrowsArgumentNullException()
    { 
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Dormitory(Guid.NewGuid(), null!));
        Assert.Equal("address", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidFloorCount_ThrowsArgumentOutRangeException(int floorCount)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => DormitoryFactory.Create(floorCount: floorCount));
        
        Assert.Equal("floorCount", exception.ParamName);
    }
    #endregion

    #region AddRoom Tests

    [Fact]
    public void AddRoom_ValidArguments_RoomAddedEventWillBeCreated()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create(floorCount: 4);
        
        var roomName = new RoomName("406");
        const Gender gender = Gender.Male;
        const int floorNumber = 4;
        const int capacity = 2;

        // Act
        var newRoomId = dormitory.AddRoom(roomName, gender, capacity, floorNumber);
        
        // Assert
        Assert.Single(dormitory.DomainEvents);

        var roomAddedEvent = Assert.IsType<RoomAddedEvent>(dormitory.DomainEvents.First());
        
        Assert.Equal(dormitory.Id, roomAddedEvent.DormitoryId);
        Assert.Equal(newRoomId, roomAddedEvent.RoomId);
        Assert.Equal(roomName, roomAddedEvent.RoomName);
        Assert.Equal(gender, roomAddedEvent.Gender);
        Assert.Equal(capacity, roomAddedEvent.Capacity);
        Assert.Equal(floorNumber, roomAddedEvent.FloorNumber);
    }

    [Fact]
    public void AddRoom_FloorNumberExceedingNumberFloors_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        
        var roomName = new RoomName("406");
        const Gender gender = Gender.Male;
        const int floorNumber = 999;
        const int capacity = 2;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dormitory.AddRoom(roomName, gender, capacity, floorNumber));
        Assert.Equal("floorNumber", exception.ParamName);
    }

    [Fact]
    public void AddRoom_RoomWithDuplicateName_ThrowsRoomAlreadyExistsException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();

        const string duplicateName = "106";
        
        var roomName = new RoomName(duplicateName);
        var duplicateRoomName = new RoomName(duplicateName);

        dormitory.AddRoom(roomName, Gender.Male, 2);
        
        // Act & Assert
        var exception = Assert.Throws<RoomAlreadyExistsException>(
            () => dormitory.AddRoom(duplicateRoomName, Gender.Male, 2));
        
        Assert.Equal(dormitory.Id, exception.DormitoryId);
        Assert.Equal(duplicateRoomName, exception.Name);
    }

    #endregion

    #region RemoveRoom Tests

    [Fact]
    public void RemoveRoom_IdentifierExistingRoom_RoomRemovedEventWillBeCreated()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        
        var roomName = new RoomName("106");
        const Gender gender = Gender.Male;
        const int capacity = 2;
        
        var newRoomId = dormitory.AddRoom(roomName, gender, capacity);
        
        // Act
        dormitory.RemoveRoom(newRoomId);
        
        // Assert
        Assert.Equal(2, dormitory.DomainEvents.Count);
        
        var roomRemovedEvent = Assert.IsType<RoomRemovedEvent>(dormitory.DomainEvents.Last());
        
        Assert.Equal(dormitory.Id, roomRemovedEvent.DormitoryId);
        Assert.Equal(newRoomId, roomRemovedEvent.RoomId);
    }
    
    [Fact]
    public void RemoveRoom_IdentifierNonExistentRoom_ThrowsNotFoundException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        
        // Act & Assert
        Assert.Throws<NotFoundException>(() => dormitory.RemoveRoom(Guid.NewGuid()));
    }

    #endregion

    #region ActivateRoom Tests

    [Fact]
    public void ActivateRoom_InactiveRoom_MakeRoomActive()
    {
        // Arrange
        var (dormitory, roomsIds) = DormitoryFactory.CreateWithRooms(roomsCount: 1);
        var roomId = roomsIds[0];
        
        dormitory.DeactivateRoom(roomId);
        
        // Act
        dormitory.ActivateRoom(roomId);
        
        // Assert
        var room = dormitory.Rooms.First(r => r.Id == roomId);
        
        Assert.True(room.IsActive);
    }

    [Fact]
    public void ActivateRoom_NonExistentRoom_ThrowsNotFoundException()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var dormitory = DormitoryFactory.Create();
        
        // Act & Asser
        var exception = Assert.Throws<NotFoundException>(() => dormitory.ActivateRoom(roomId));

        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(roomId, exception.EntityId);
    }

    #endregion

    #region DeactivateRoom Tests

    [Fact]
    public void DeactivateRoom_ActiveRoom_MakeRoomInactive()
    {
        // Arrange
        var (dormitory, roomsIds) = DormitoryFactory.CreateWithRooms(roomsCount: 1);
        var roomId = roomsIds[0];

        // Act
        dormitory.DeactivateRoom(roomId);
        
        // Assert
        var room = dormitory.Rooms.First(r => r.Id == roomId);
        
        Assert.False(room.IsActive);
    }

    [Fact]
    public void DeactivateRoom_NonExistentRoom_ThrowsNotFoundException()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var dormitory = DormitoryFactory.Create();
        
        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => dormitory.DeactivateRoom(roomId));
        
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(roomId, exception.EntityId);
    }

    #endregion
}