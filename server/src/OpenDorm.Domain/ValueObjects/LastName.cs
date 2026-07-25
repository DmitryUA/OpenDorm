using System.Text.RegularExpressions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record LastName
{
    private static readonly Regex LastNameRegex = new(
        @"^[А-ЯA-ZЁ][а-яa-zё]+$", 
        RegexOptions.Compiled
    );
    
    public const int MinLength = 2;
    public const int MaxLength = 30;
    public string Value { get; init; }

    public LastName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "Last name cannot be empty or white space");

        var trimmedValue = value.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmedValue.Length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedValue.Length, MaxLength);

        if (!LastNameRegex.IsMatch(trimmedValue))
            throw new DomainException(
                "Last name must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.");
        
        Value = trimmedValue;
    }
}