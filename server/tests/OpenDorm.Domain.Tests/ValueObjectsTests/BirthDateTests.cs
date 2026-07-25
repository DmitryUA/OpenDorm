using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class BirthDateTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidValue_CreateInstanceAndPreservesValue()
    {
        // Arrange
        var expected = new DateOnly(2003, 1, 21);
        
        // Act
        var birthDate = new BirthDate(expected);
        
        // Assert
        Assert.Equal(expected, birthDate.Value);
    }

    [Fact]
    public void Constructor_AgeExceedsTheMaximumLimit_ThrowsDomainException()
    {
        // Arrange
        var date = new DateOnly(1800, 1, 1);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => new BirthDate(date));
    }

    [Fact]
    public void Constructor_DateThatHasNotYetArrived_ThrowsDomainException()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        
        // Act & Assert
        Assert.Throws<DomainException>(() => new BirthDate(date));
    }

    #endregion

    #region CalculateAge Tests

    [Fact]
    public void CalculateAge_BirthdayAlreadyPassedThisYear_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1990, 1, 15));
        var currentDate = new DateOnly(2026, 7, 12);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(36, age);
    }
    
    [Fact]
    public void CalculateAge_BirthdayNotYetPassedThisYear_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1990, 12, 25));
        var currentDate = new DateOnly(2026, 7, 12);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(35, age);
    }
    
    [Fact]
    public void CalculateAge_BirthdayIsToday_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1990, 7, 12));
        var currentDate = new DateOnly(2026, 7, 12);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(36, age);
    }
    
    [Fact]
    public void CalculateAge_BirthdayTomorrow_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1990, 7, 13));
        var currentDate = new DateOnly(2026, 7, 12);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(35, age);
    }
    
    [Fact]
    public void CalculateAge_LeapYearBirthday_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1988, 2, 29));
        var currentDate = new DateOnly(2026, 3, 1);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(38, age);
    }
    
    [Fact]
    public void CalculateAge_LeapYearBirthday_NotYetPassed_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(1988, 2, 29));
        var currentDate = new DateOnly(2026, 2, 27);

        var age = birthDate.CalculateAge(currentDate);
    }
    
    [Fact]
    public void CalculateAge_NewYearEve_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(2000, 12, 31));
        var currentDate = new DateOnly(2025, 12, 31);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(25, age);
    }
    
    [Fact]
    public void CalculateAge_NewYear_ReturnsCorrectAge()
    {
        var birthDate = new BirthDate(new DateOnly(2000, 12, 31));
        var currentDate = new DateOnly(2026, 1, 1);

        var age = birthDate.CalculateAge(currentDate);

        Assert.Equal(25, age);
    }
    
    [Fact]
    public void CalculateAge_JustBorn_ReturnsZero()
    {
        var today = new DateOnly(2026, 7, 12);
        var birthDate = new BirthDate(today);

        var age = birthDate.CalculateAge(today);

        Assert.Equal(0, age);
    }

    #endregion
}