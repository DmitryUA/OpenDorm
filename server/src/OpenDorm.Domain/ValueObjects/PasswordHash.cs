using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record PasswordHash
{
    public const int MaxLength = 255;
    public string Value { get; }

    public PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPasswordHashException("Password hash cannot be empty.");

        if (value.Length > MaxLength)
            throw new InvalidPasswordHashException($"Password hash cannot exceed {MaxLength} characters.");

        Value = value;
    }
}