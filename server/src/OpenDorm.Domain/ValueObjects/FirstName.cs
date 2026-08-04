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
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed)) throw new InvalidFirstNameException("First name cannot be empty.");
        if (trimmed.Length < MinLength)
            throw new InvalidFirstNameException($"First name cannot be less than {MinLength} characters long.");
        if (trimmed.Length > MaxLength)
            throw new InvalidFirstNameException($"First name cannot be exceed {MaxLength} characters.");
        
        if (!FirstNameRegex.IsMatch(trimmed))
            throw new InvalidFirstNameException(
                "First name must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.");
        
        Value = trimmed;
    }
}