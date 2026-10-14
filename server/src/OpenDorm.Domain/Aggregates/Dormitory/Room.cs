using OpenDorm.Domain.Common;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Dormitory;

public class Room : Entity
{
    public RoomName Name { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public bool IsActive { get; private set; } = true;
    public RoomCapacity Capacity { get; private set; } = null!;
    public FloorNumber FloorNumber { get; private set; } = null!;

    // ReSharper disable once UnusedMember.Local
    private Room(){} // For EF Core only
    public Room(Guid id, RoomName name, Gender gender, RoomCapacity capacity, FloorNumber floorNumber) : base(id)
    {
        Name = name ?? throw new InvalidRoomException("Room name cannot be null.");
        Gender = gender;
        Capacity = capacity ?? throw new InvalidRoomException("Room capacity cannot be null.");
        FloorNumber = floorNumber ?? throw new InvalidRoomException("Room floor number cannot be null.");
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

    public void ChangeName(RoomName name) => Name = name;
    public void ChangeGender(Gender gender) => Gender = gender;
    public void ChangeCapacity(RoomCapacity capacity) => Capacity = capacity;
    public void ChangeFloorNumber(FloorNumber floorNumber) => FloorNumber = floorNumber;
}