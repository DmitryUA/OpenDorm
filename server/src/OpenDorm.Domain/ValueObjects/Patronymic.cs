using System.Text.RegularExpressions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public class Patronymic
{
    public const int MinLength = 2;
    public const int MaxLength = 30;
    
    private static readonly Regex PatronymicRegex = new(
        @"^[А-ЯA-ZЁ][а-яa-zё]+$", 
        RegexOptions.Compiled
    );
    public string Value { get; }

    public Patronymic(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "Patronymic cannot be empty or white space");

        var trimmedValue = value.Trim();
        
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmedValue.Length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedValue.Length, MaxLength);

        if (!PatronymicRegex.IsMatch(trimmedValue))
            throw new DomainException(
                "Patronymic must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.");
        
        Value = trimmedValue;
    }
}