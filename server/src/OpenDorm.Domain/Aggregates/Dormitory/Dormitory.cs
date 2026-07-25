using OpenDorm.Domain.Common;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Dormitory;

public class Dormitory : AggregateRoot
{
    public Address Address { get; init; }
    public int FloorCount { get; init; }

    private readonly List<Room> _rooms = [];

    public Dormitory(Guid id, Address address, int floorCount = 1) : base(id)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentOutOfRangeException.ThrowIfLessThan(floorCount, 1);
        
        Address = address;
        FloorCount = floorCount;
    }

    public Guid AddRoom(RoomName name, Gender gender, int capacity = 1, int floorNumber = 1)
    {
        if (floorNumber > FloorCount)
            throw new ArgumentOutOfRangeException(nameof(floorNumber), $"There is no floor number {floorNumber} in the dormitory.");

        if (_rooms.Any(r => r.Name == name)) throw new RoomAlreadyExistsException(Id ,name);
        
        var room = new Room(Guid.NewGuid(), name, gender, capacity, floorNumber);
        _rooms.Add(room);

        var roomAddedEvent = new RoomAddedEvent(
            Id,
            room.Id,
            room.Name,
            room.Gender,
            room.Capacity,
            room.FloorNumber);
        
        AddDomainEvent(roomAddedEvent);

        return room.Id;
    }

    public void RemoveRoom(Guid roomId)
    {
        var roomIndex = _rooms.FindIndex(r => r.Id == roomId);

        if (roomIndex == -1)
            throw new DomainException($"Room with id '{roomId}' not found in dormitory '{Id}");

        var roomRemovedEvent = new RoomRemovedEvent(Id, roomId);
        AddDomainEvent(roomRemovedEvent);
                
        _rooms.RemoveAt(roomIndex);
    }

    public void RemoveRoom(RoomName name)
    {
        var roomIndex = _rooms.FindIndex(r => r.Name == name);

        if (roomIndex == -1)
            throw new DomainException($"Room with name '{name}' not found in dormitory '{Id}");

        var roomId = _rooms[roomIndex].Id;
        var roomRemovedEvent = new RoomRemovedEvent(Id, roomId);
        AddDomainEvent(roomRemovedEvent);
        
        _rooms.RemoveAt(roomIndex);
    }

    public void CheckIn(RoomName roomName, Guid occupantId)
    {
        CheckIn(r => r.Name == roomName, $"Room '{roomName}'", occupantId);
    }

    public void CheckIn(Guid roomId, Guid occupantId)
    {
        CheckIn(r => r.Id == roomId, $"Room with id '{roomId}'", occupantId);
    }

    private void CheckIn(Func<Room, bool> predicate, string roomDescription, Guid occupantId)
    {
        var room = _rooms.FirstOrDefault(predicate);
    
        if (room is null)
            throw new DomainException($"{roomDescription} was not found in dormitory. Dormitory id: '{Id}'");
    
        room.CheckIn(occupantId);
        AddDomainEvent(new RoomOccupiedEvent(Id, room.Id, occupantId));
    }

    public void CheckOut(Guid occupantId)
    {
        var room = _rooms.FirstOrDefault(r => r.OccupantIds.Contains(occupantId));

        if (room == null)
            throw new DomainException(
                $"Occupant with id: '{occupantId}' was not found in dormitory. Dormitory id: '{Id}'");
        
        room.CheckOut(occupantId);

        var occupantCheckedOutEvent = new RoomVacatedEvent(Id, room.Id, occupantId);
        AddDomainEvent(occupantCheckedOutEvent);
    }
}