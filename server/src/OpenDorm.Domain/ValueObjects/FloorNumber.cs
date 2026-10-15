using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record FloorNumber
{
    public int Value { get; init; }

    public FloorNumber(int value)
    {
        if (value < 1) throw new InvalidFloorNumberException("Floor number cannot be less than 1.");

        Value = value;
    }

    public override string ToString() => Value.ToString();
}