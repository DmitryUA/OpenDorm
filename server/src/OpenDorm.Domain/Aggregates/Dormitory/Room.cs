using OpenDorm.Domain.Common;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Dormitory;

public class Room : Entity
{
    public int Capacity { get; init; }
    public RoomName Name { get; init; }
    public Gender Gender { get; init; }
    public int FloorNumber { get; init; }
    public bool HasVacancy => _occupantIds.Count < Capacity;
    private List<Guid> _occupantIds = [];
    public IReadOnlyCollection<Guid> OccupantIds => _occupantIds.AsReadOnly();

    public void CheckIn(Guid occupantId)
    {
        if (!HasVacancy)
            throw new RoomFullyOccupiedException(Id, Name);

        if (_occupantIds.Contains(occupantId))
            throw new DuplicateOccupantException(occupantId, Id, Name);
        
        _occupantIds.Add(occupantId);
    }

    public void CheckOut(Guid occupantId)
    {
        if (!_occupantIds.Remove(occupantId))
            throw new DomainException($"Occupant is not in this room. Room id: '{Id}', occupant id: '{occupantId}'");
    }

    public Room(Guid id, RoomName name, Gender gender, int capacity = 1, int floorNumber = 1) : base(id)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(floorNumber, 1);

        Name = name;
        Gender = gender;
        Capacity = capacity;
        FloorNumber = floorNumber;
    }
}