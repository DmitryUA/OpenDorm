using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class AddressTests
{
    [Fact]
    public void Ctor_WithStreetIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        City city = new("Москва");
        
        // Act
        var act = () => new Address(city, null!);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("street", exception.ParamName);
    }

    [Fact]
    public void Ctor_WithCityIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Street street = new("Тверская");
        
        // Act
        var act = () => new Address(null!, street);
        
        // Assert
        var ex = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("city", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithBothAreValid_CreateInstanceSuccessfully()
    {
        // Arrange
        City city = new("Москва");
        Street street = new("Тверская");
        
        // Act
        var address = new Address(city, street);
        
        // Assert
        Assert.Equal(city, address.City);
        Assert.Equal(street, address.Street);
    }

    [Fact]
    public void ToString_ReturnsCityAndStreetSeparateByComma()
    {
        // Arrange
        const string cityName = "Москва";
        const string streetName = "Тверская";
        const string expected = $"{cityName}, {streetName}";
        
        City city = new(cityName);
        Street street = new(streetName);

        var address = new Address(city, street);
        
        // Act
        var result = address.ToString();
        
        // Assert
        Assert.Equal(expected, result);
    }
}