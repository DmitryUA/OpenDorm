using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.ValueObjects;

public record BirthDate
{
    public const int MaxAgeYears = 140;

    public DateOnly Value { get; }

    public BirthDate(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date > today)
            throw new InvalidBirthDateException("Date of birth cannot be in the future.");
        
        var maxAllowedDate = today.AddYears(-MaxAgeYears);
        if (date < maxAllowedDate)
            throw new InvalidBirthDateException($"Age cannot exceed {MaxAgeYears} years.");

        Value = date;
    }

    public int CalculateAge() => CalculateAgeByCurrentDate(DateOnly.FromDateTime(DateTime.UtcNow));

    public int CalculateAgeByCurrentDate(DateOnly currentDate) => CalculateAgeByRange(Value, currentDate);

    public static int CalculateAgeByBirthDate(DateOnly birthDate) =>
        CalculateAgeByRange(birthDate, DateOnly.FromDateTime(DateTime.UtcNow));

    private static int CalculateAgeByRange(DateOnly birthDate, DateOnly currentDate)
    {
        var age = currentDate.Year - birthDate.Year;

        if (currentDate < birthDate.AddYears(age))
            age--;

        return age;
    }

    public override string ToString() => Value.ToString();
}