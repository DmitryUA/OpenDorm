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
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new InvalidPatronymicException("Patronymic cannot be empty.");
        if (trimmed.Length < MinLength)
            throw new InvalidPatronymicException($"Patronymic cannot be less than {MinLength} characters long.");
        if (trimmed.Length > MaxLength)
            throw new InvalidPatronymicException($"Patronymic cannot be exceed {MaxLength} characters.");

        if (!PatronymicRegex.IsMatch(trimmed))
            throw new InvalidPatronymicException(
                "Patronymic must consist only of letters. First letter of each part must be uppercase, and the rest lowercase.");
        
        Value = trimmed;
    }
}