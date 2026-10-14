using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record RoomCapacity
{
    public int Value { get; init; }

    public RoomCapacity(int value)
    {
        if (value < 1) throw new InvalidRoomCapacityException("Room capacity cannot be less than 1.");

        Value = value;
    }
}