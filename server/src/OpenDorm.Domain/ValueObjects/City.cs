namespace OpenDorm.Domain.ValueObjects;

public sealed record City
{
    public const byte MaxNameLength = 50;
    public const byte MinNameLength = 3;
    public string Name { get; }

    public City(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmed.Length, MinNameLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, MaxNameLength, nameof(name));

        Name = trimmed;
    }
}