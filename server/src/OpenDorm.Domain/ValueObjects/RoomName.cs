using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public sealed record RoomName
{
    public const int MaxLength = 128;
    public const int MinLength = 1;
    public string Value { get; }

    public RoomName(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidRoomNameException("Room name cannot be empty.");
        if (trimmed.Length < MinLength)
            throw new InvalidRoomNameException($"Room name cannot be less than {MinLength} characters long.");
        if (trimmed.Length > MaxLength)
            throw new InvalidRoomNameException($"Room name cannot be exceed {MaxLength} characters.");
        
        Value = trimmed;
    }

    public override string ToString() => Value;
}