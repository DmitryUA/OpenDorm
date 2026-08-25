using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public sealed record Street
{
    public const byte MaxLength = 255;
    public const byte MinLength = 3;
    public string Value { get; }

    public Street(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidStreetException("Street name cannot be empty.");
        if (trimmed.Length < MinLength)
            throw new InvalidStreetException(
                $"Street name cannot be less than {MinLength} characters long.");
        if (trimmed.Length > MaxLength)
            throw new InvalidStreetException($"Street name cannot exceed {MaxLength} characters.");

        Value = trimmed;
    }

    public override string ToString() => Value;
}