using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public sealed record HouseNumber
{
    public const int MaxLength = 20;
    public string Value { get; }

    public HouseNumber(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidHouseNumberException("House number cannot be empty.");
        if (trimmed.Length > MaxLength)
            throw new InvalidHouseNumberException($"House number cannot exceed {MaxLength} characters.");

        Value = trimmed;
    }
}