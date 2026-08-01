using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class CityTests
{
    [Theory]
    [InlineData("Уфа")]
    [InlineData("Москва")]
    [InlineData("Александровск-Сахалинский")]
    public void Ctor_ValidValue_CreateInstanceAndPreservesValue(string name)
    { 
        // Arrange & Act
        var city = new City(name);

        // Assert
        Assert.Equal(name, city.Value);
    }

    [Fact]
    public void Ctor_LeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string name = " Москва  ";
        const string expectedName = "Москва";
        
        // Act
        var city = new City(name);
        
        // Assert
        Assert.Equal(expectedName, city.Value);
    }
    
    [Fact]
    public void Ctor_MinValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('A', City.MinLength);
        
        // Act
        var city = new City(minValidName);
        
        // Assert
        Assert.Equal(minValidName, city.Value);
    }

    [Fact]
    public void Ctor_MaxValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('A', City.MaxLength);
        
        // Act
        var city = new City(maxValidName);
        
        // Assert
        Assert.Equal(maxValidName, city.Value);
    }

    [Fact]
    public void Ctor_Null_ThrowsInvalidCityException()
    {
        // Arrange
        const string expectedMessage = "City name cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidCityException>(() => new City(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_EmptyOrWhitespace_ThrowsInvalidCityException(string value)
    {
        // Arrange
        const string expectedMessage = "City name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidCityException>(() => new City(value));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public void Ctor_LengthLessThanMin_ThrowsInvalidCityException(string value)
    {
        // Arrange
        var expectedMessage = $"City name cannot be less than {City.MinLength} characters long.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidCityException>(() => new City(value));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Ctor_LengthGreaterThanMax_ThrowsInvalidCityException()
    {
        // Arrange
        var longName = new string('A', City.MaxLength + 1);
        var expectedMessage = $"City name cannot be exceed {City.MaxLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidCityException>(() => new City(longName));
        Assert.Equal(expectedMessage, exception.Message);
    }
}