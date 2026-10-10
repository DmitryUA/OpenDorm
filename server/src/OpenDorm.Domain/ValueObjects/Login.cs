using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public partial record Login
{
    public const int MinLength = 4;
    public const int MaxLength = 20;
    public string Value { get; }

    public Login(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidLoginException("Login cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length is < MinLength or > MaxLength)
            throw new InvalidLoginException(
                $"Login length must be between {MinLength} and {MaxLength} characters. Actual: {normalized.Length}.");

        if (!MyRegex().IsMatch(normalized))
            throw new InvalidLoginException(
                "Login can contain only lowercase letters (a-z), digits (0-9), and symbols '_', '-', '.'.");

        Value = normalized;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-z0-9_.\-]+$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}