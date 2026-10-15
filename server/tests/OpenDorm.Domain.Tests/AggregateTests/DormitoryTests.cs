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
    public void Constructor_NullAddress_ThrowsInvalidDormitoryException()
    {
        // Arrange
        const string expectedMessage = "Dormitory address cannot be null.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidDormitoryException>(() => new Dormitory(Guid.NewGuid(), null!));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidFloorCount_ThrowsInvalidDormitoryException(int floorCount)
    {
        // Arrange
        const string expectedMessage = "Floor count cannot be less than 1.";
        // Act & Assert
        var exception = Assert.Throws<InvalidDormitoryException>(
            () => DormitoryFactory.Create(floorCount: floorCount));
        
        Assert.Equal(expectedMessage, exception.Message);
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
        var floorNumber = new FloorNumber(4);
        var capacity = new RoomCapacity(2);

        // Act
        var newRoomId = dormitory.AddRoom(roomName, gender, capacity, floorNumber);
        
        // Assert
        Assert.Single(dormitory.DomainEvents);

        var roomAddedEvent = Assert.IsType<RoomAddedEvent>(dormitory.DomainEvents.First());
        
        Assert.Equal(dormitory.Id, roomAddedEvent.DormitoryId);
        Assert.Equal(newRoomId, roomAddedEvent.RoomId);
        Assert.Equal(roomName, roomAddedEvent.RoomName);
        Assert.Equal(gender, roomAddedEvent.Gender);
        Assert.Equal(capacity.Value, roomAddedEvent.Capacity);
        Assert.Equal(floorNumber.Value, roomAddedEvent.FloorNumber);
    }

    [Fact]
    public void AddRoom_FloorNumberExceedingNumberFloors_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        
        var roomName = new RoomName("406");
        const Gender gender = Gender.Male;
        var floorNumber = new FloorNumber(999);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dormitory.AddRoom(
            roomName,
            gender,
            new RoomCapacity(2),
            floorNumber));
        
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

        dormitory.AddRoom(
            roomName,
            Gender.Male,
            new RoomCapacity(2),
            new FloorNumber(1));
        
        // Act & Assert
        var exception = Assert.Throws<RoomAlreadyExistsException>(
            () => dormitory.AddRoom(duplicateRoomName, Gender.Male, new RoomCapacity(2), new FloorNumber(1)));
        
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
        
        var newRoomId = dormitory.AddRoom(
            roomName,
            gender,
            new RoomCapacity(2),
            new FloorNumber(1));
        
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
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
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
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        
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

    #region UpdateRoom Tests

    [Fact]
    public void UpdateRoom_WhenRoomExists_AndUpdateIsValid_ShouldUpdateRoomProperties()
    {
        // Arrange
        var dormitory = new Dormitory(Guid.NewGuid(), AddressFactory.Create(), floorCount: 5);
        var roomId = dormitory.AddRoom(new RoomName("406"), Gender.Male, new RoomCapacity(2), new FloorNumber(4));
        var newRoomName = new RoomName("407");
        var newCapacity = new RoomCapacity(4);
        const Gender newRoomGender = Gender.Female;

        // Act
        dormitory.UpdateRoom(roomId, room =>
        {
            room.ChangeName(newRoomName);
            room.ChangeGender(newRoomGender);
            room.ChangeCapacity(newCapacity);
        });

        // Assert
        var updatedRoom = dormitory.Rooms.First(r => r.Id == roomId);
        Assert.Equal(newRoomName, updatedRoom.Name);
        Assert.Equal(newCapacity, updatedRoom.Capacity);
        Assert.Equal(newRoomGender, updatedRoom.Gender);
    }

    [Fact]
    public void UpdateRoom_WhenRoomDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        var nonExistentRoomId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() =>
            dormitory.UpdateRoom(nonExistentRoomId, _ => { }));
        
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(nonExistentRoomId, exception.EntityId);
    }

    [Fact]
    public void UpdateRoom_WhenUpdateActionIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            dormitory.UpdateRoom(roomId, null!));
    }

    #endregion

    #region ChangeAddress Tests

    [Fact]
    public void ChangeAddress_Address_UpdateAddress()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        var newAddress = AddressFactory.Random();
        
        // Act
        dormitory.ChangeAddress(newAddress);
        
        // Assert
        Assert.Equal(newAddress, dormitory.Address);
    }

    [Fact]
    public void ChangeAddress_NullAddress_ThrowsInvalidDormitoryException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        const string expectedMessage = "Dormitory address cannot be null.";

        // Act & Assert
        var exception = Assert.Throws<InvalidDormitoryException>(() => dormitory.ChangeAddress(null!));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion
}