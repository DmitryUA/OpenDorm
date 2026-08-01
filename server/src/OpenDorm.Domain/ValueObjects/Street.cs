using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public sealed record Street
{
    public const byte MaxStreetNameLength = 255;
    public const byte MinStreetNameLength = 3;
    public string Value { get; }

    public Street(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidStreetException("Street name cannot be empty.");
        if (trimmed.Length < MinStreetNameLength)
            throw new InvalidStreetException(
                $"Street name cannot be less than {MinStreetNameLength} characters long.");
        if (trimmed.Length > MaxStreetNameLength)
            throw new InvalidStreetException($"Street name cannot exceed {MaxStreetNameLength} characters.");

        Value = trimmed;
    }

    public override string ToString() => Value;
}