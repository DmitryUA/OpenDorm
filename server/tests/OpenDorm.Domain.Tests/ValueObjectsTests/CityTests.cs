using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class CityTests
{
    [Theory]
    [InlineData("Уфа")]
    [InlineData("Москва")]
    [InlineData("Александровск-Сахалинский")]
    public void Ctor_WithValidName_CreateInstanceAndPreservesName(string name)
    { 
        // Arrange & Act
        var city = new City(name);

        // Assert
        Assert.Equal(name, city.Name);
    }

    [Fact]
    public void Ctor_WithLeadingAndTrailingWhitespace_TrimsName()
    {
        // Arrange
        const string name = " Москва  ";
        const string expectedName = "Москва";
        
        // Act
        var city = new City(name);
        
        // Assert
        Assert.Equal(expectedName, city.Name);
    }

    [Fact]
    public void Ctor_withNull_ThrowsArgumentNullException()
    {
        // Arrange
        const string? nullName = null;

        // Act
        var act = () => new City(nullName!);

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_WithEmptyOrWhitespace_ThrowArgumentException(string name)
    {
        // Arrange & Act
        var act = () => new City(name);
        
        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public void Ctor_WithLengthLessThanMin_ThrowsArgumentOutOfRangeException(string name)
    {
        // Arrange & Act
        var act = () => new City(name);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Ctor_WithLengthGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var longName = new string('A', City.MaxNameLength + 1);
        
        // Act
        var act = () => new City(longName);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Ctor_WithMinValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('A', City.MinNameLength);
        
        // Act
        var city = new City(minValidName);
        
        // Assert
        Assert.Equal(minValidName, city.Name);
    }

    [Fact]
    public void Ctor_WithMaxValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('A', City.MaxNameLength);
        
        // Act
        var city = new City(maxValidName);
        
        // Assert
        Assert.Equal(maxValidName, city.Name);
    }
}