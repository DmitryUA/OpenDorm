using OpenDorm.Domain.Common;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Dormitory;

public class Room : Entity
{
    public int Capacity { get; private set; }
    public RoomName Name { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public int FloorNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    // ReSharper disable once UnusedMember.Local
    private Room(){} // For EF Core only
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
    
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
    }
}