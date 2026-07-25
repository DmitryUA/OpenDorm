using System.Text.RegularExpressions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record FirstName
{
    private static readonly Regex FirstNameRegex = new(
        @"^[А-ЯA-ZЁ][а-яa-zё]+$", 
        RegexOptions.Compiled
    );
    
    public const int MinLength = 2;
    public const int MaxLength = 30;
    public string Value  { get; init; }

    public FirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "First name cannot be empty or white space");

        var trimmedValue = value.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmedValue.Length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedValue.Length, MaxLength);

        if (!FirstNameRegex.IsMatch(trimmedValue))
            throw new DomainException(
                "Name must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.");
        
        Value = trimmedValue;
    }
}