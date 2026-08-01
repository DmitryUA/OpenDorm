using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public sealed record City
{
    public const byte MaxLength = 50;
    public const byte MinLength = 3;
    public string Value { get; }

    public City(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidCityException("City name cannot be empty.");
        if (trimmed.Length < MinLength)
            throw new InvalidCityException($"City name cannot be less than {MinLength} characters long.");
        if (trimmed.Length > MaxLength)
            throw new InvalidCityException($"City name cannot be exceed {MaxLength} characters.");

        Value = trimmed;
    }

    public override string ToString() => Value;
}