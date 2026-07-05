using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.AggregateTests;

public class DormitoryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidArguments_CreateDormitoryWithValidProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = new Address(new City("Москва"), new Street("Советская"));
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
        var address = new Address(new City("Москва"), new Street("Советская"));
        
        // Act
        var dormitory = new Dormitory(id, address);

        // Assert
        Assert.Equal(1, dormitory.FloorCount);
    }

    [Fact]
    public void Constructor_NullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new DormitoryBuilder().WithAddress(null!);
        
        // Act
        var act = builder.Build;
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("address", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidFloorCount_ThrowsArgumentOutRangeException(int floorCount)
    {
        // Arrange
        var dormitoryBuilder = new DormitoryBuilder().WithFloorCount(floorCount);
        
        // Act
        var act = dormitoryBuilder.Build;
        
        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("floorCount", exception.ParamName);
    }
    #endregion

    #region AddRoom Tests

    [Fact]
    public void AddRoom_ValidArguments_RoomAddedEventWillBeCreated()
    {
        // Arrange
        var dormitory = new DormitoryBuilder().Build();
        
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
        var dormitory = new DormitoryBuilder().Build();
        
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
        var dormitory = new DormitoryBuilder().Build();

        const string duplicateName = "406";
        
        var roomName = new RoomName(duplicateName);
        var duplicateRoomName = new RoomName(duplicateName);

        dormitory.AddRoom(roomName, Gender.Male, 2, 4);
        
        // Act & Assert
        var exception = Assert.Throws<RoomAlreadyExistsException>(
            () => dormitory.AddRoom(duplicateRoomName, Gender.Male, 2, 4));
        
        Assert.Equal(dormitory.Id, exception.DormitoryId);
        Assert.Equal(duplicateRoomName, exception.Name);
    }

    #endregion

    #region RemoveRoom Tests

    [Fact]
    public void RemoveRoom_IdentifierExistingRoom_RoomRemovedEventWillBeCreated()
    {
        // Arrange
        var dormitory = new DormitoryBuilder().Build();
        
        var roomName = new RoomName("406");
        const Gender gender = Gender.Male;
        const int floorNumber = 4;
        const int capacity = 2;
        
        var newRoomId = dormitory.AddRoom(roomName, gender, capacity, floorNumber);
        
        // Act
        dormitory.RemoveRoom(newRoomId);
        
        // Assert
        Assert.Equal(2, dormitory.DomainEvents.Count);
        
        var roomRemovedEvent = Assert.IsType<RoomRemovedEvent>(dormitory.DomainEvents.Last());
        
        Assert.Equal(dormitory.Id, roomRemovedEvent.DormitoryId);
        Assert.Equal(newRoomId, roomRemovedEvent.RoomId);
    }

    [Fact]
    public void RemoveRoom_NameExistingRoom_RoomRemovedEventWillBeCreated()
    {
        // Arrange
        var dormitory = new DormitoryBuilder().Build();
        
        var roomName = new RoomName("406");
        const Gender gender = Gender.Male;
        const int floorNumber = 4;
        const int capacity = 2;
        
        var newRoomId = dormitory.AddRoom(roomName, gender, capacity, floorNumber);
        
        // Act
        dormitory.RemoveRoom(roomName);
        
        // Assert
        Assert.Equal(2, dormitory.DomainEvents.Count);
        
        var roomRemovedEvent = Assert.IsType<RoomRemovedEvent>(dormitory.DomainEvents.Last());
        
        Assert.Equal(dormitory.Id, roomRemovedEvent.DormitoryId);
        Assert.Equal(newRoomId, roomRemovedEvent.RoomId);
    }
    
    [Fact]
    public void RemoveRoom_IdentifierNonExistentRoom_ThrowsDomainException()
    {
        // Arrange
        var dormitory = new DormitoryBuilder().Build();
        
        // Act & Assert
        Assert.Throws<DomainException>(() => dormitory.RemoveRoom(Guid.NewGuid()));
    }
    
    [Fact]
    public void RemoveRoom_NameNonExistentRoom_ThrowsDomainException()
    {
        // Arrange
        var dormitory = new DormitoryBuilder().Build();
        var nameNonExistentRoom = new RoomName("406");
        
        // Act & Assert
        Assert.Throws<DomainException>(() => dormitory.RemoveRoom(nameNonExistentRoom));
    }

    #endregion
    
    private class DormitoryBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Address _address = new(new City("Москва"), new Street("Советская"));
        private int _floorCount = 9;

        public DormitoryBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public DormitoryBuilder WithAddress(Address address)
        {
            _address = address;
            return this;
        }

        public DormitoryBuilder WithFloorCount(int floorCount)
        {
            _floorCount = floorCount;
            return this;
        }

        public Dormitory Build() => new(_id, _address, _floorCount);
    }
}