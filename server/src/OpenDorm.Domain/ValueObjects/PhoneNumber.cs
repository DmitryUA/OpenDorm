using System.Text.RegularExpressions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public partial record PhoneNumber
{
    public readonly string Value;

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPhoneNumber("Phone number cannot be null or empty.");

        var cleaned = Normalize(value);

        if (!IsValid(cleaned))
            throw new InvalidPhoneNumber("Invalid phone number format. Expected +7 followed by 10 digits.");

        Value = cleaned;
    }

    private static string Normalize(string input)
    {
        var digits = DigitsOnlyRegex().Replace(input, "");

        return digits.Length switch
        {
            11 when digits.StartsWith("7") || digits.StartsWith("8") => $"+7{digits[1..]}",
            10 => $"+7{digits}",
            _ => $"+{digits}"
        };
    }

    private static bool IsValid(string input) =>
        ValidPhoneRegex().IsMatch(input);

    public override string ToString() => Value;
    
    [GeneratedRegex(@"\D", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyRegex();
    
    [GeneratedRegex(@"^\+7\d{10}$", RegexOptions.Compiled)]
    private static partial Regex ValidPhoneRegex();
}