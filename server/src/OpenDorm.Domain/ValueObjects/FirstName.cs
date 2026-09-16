using System.Text.RegularExpressions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public partial record FirstName
{
    public const int MinLength = 2;
    public const int MaxLength = 30;
    public static readonly Regex FirstNameRegex = MyRegex();
    
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

    public override string ToString() => Value;
    
    [GeneratedRegex(@"^[А-ЯA-ZЁ][а-яa-zё]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}