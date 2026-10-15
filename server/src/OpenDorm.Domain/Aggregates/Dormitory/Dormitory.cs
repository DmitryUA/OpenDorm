using OpenDorm.Domain.Common;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Dormitory;

public class Dormitory : AggregateRoot
{
    public Address Address { get; private set; } = null!;
    public int FloorCount { get; }
    private readonly List<Room> _rooms = [];
    
    internal IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();
    
    // ReSharper disable once UnusedMember.Local
    private Dormitory() {}  // For EF Core only
    public Dormitory(Guid id, Address address, int floorCount = 1) : base(id)
    {
        if (floorCount < 1) throw new InvalidDormitoryException("Floor count cannot be less than 1.");
        
        Address = address ?? throw new InvalidDormitoryException("Dormitory address cannot be null.");
        FloorCount = floorCount;
    }

    public Guid AddRoom(RoomName name, Gender gender, RoomCapacity capacity, FloorNumber floorNumber)
    {
        if (floorNumber.Value > FloorCount)
            throw new ArgumentOutOfRangeException(nameof(floorNumber), $"There is no floor number {floorNumber} in the dormitory.");

        if (_rooms.Any(r => r.Name == name)) throw new RoomAlreadyExistsException(Id ,name);
        
        var room = new Room(Guid.NewGuid(), name, gender, capacity, floorNumber);
        _rooms.Add(room);

        var roomAddedEvent = new RoomAddedEvent(
            Id,
            room.Id,
            room.Name,
            room.Gender,
            room.Capacity.Value,
            room.FloorNumber.Value);
        
        AddDomainEvent(roomAddedEvent);

        return room.Id;
    }
    public void RemoveRoom(Guid id)
    {
        var roomIndex = _rooms.FindIndex(r => r.Id == id);

        if (roomIndex == -1)
            throw new NotFoundException(nameof(Room), id);

        var roomRemovedEvent = new RoomRemovedEvent(Id, id);
        AddDomainEvent(roomRemovedEvent);
                
        _rooms.RemoveAt(roomIndex);
    }
    public void DeactivateRoom(Guid id)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == id);

        if (room == null) throw new NotFoundException(nameof(Room), id);
        
        room.Deactivate();
    }
    public void ActivateRoom(Guid id)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == id);

        if (room == null) throw new NotFoundException(nameof(Room), id);
        
        room.Activate();
    }
    public void ChangeAddress(Address address) =>
        Address = address ?? throw new InvalidDormitoryException("Dormitory address cannot be null.");
}