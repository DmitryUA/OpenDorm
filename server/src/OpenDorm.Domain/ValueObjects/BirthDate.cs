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
            throw new DomainException("Date of birth cannot be in the future.");
        
        var maxAllowedDate = today.AddYears(-MaxAgeYears);
        if (date < maxAllowedDate)
            throw new DomainException($"Age cannot exceed {MaxAgeYears} years.");

        Value = date;
    }

    public int CalculateAge()
    {
        return CalculateAge(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public int CalculateAge(DateOnly currentDate)
    {
        int age = currentDate.Year - Value.Year;
    
        if (currentDate < Value.AddYears(age)) 
            age--;
        
        return age;
    }
    
    public static implicit operator DateOnly(BirthDate date) => date.Value;
    public static implicit operator BirthDate(DateOnly date) => new(date);
}