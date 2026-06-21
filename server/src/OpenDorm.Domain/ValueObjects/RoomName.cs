namespace OpenDorm.Domain.ValueObjects;

public sealed record RoomName
{
    public const int MaxNameLength = 128;
    public const int MinNameLength = 1;
    public string Name { get; }

    public RoomName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmed.Length, MinNameLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, MaxNameLength, nameof(name));

        Name = trimmed;
    }
}