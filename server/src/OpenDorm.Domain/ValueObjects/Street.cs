namespace OpenDorm.Domain.ValueObjects;

public sealed record Street
{
    public const byte MaxStreetNameLength = 255;
    public const byte MinStreetNameLength = 3;
    public string Name { get; }

    public Street(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmed.Length, MinStreetNameLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, MaxStreetNameLength, nameof(name));

        Name = trimmed;
    }
}