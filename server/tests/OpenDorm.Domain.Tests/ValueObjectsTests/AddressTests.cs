using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class AddressTests
{
    [Fact]
    public void Ctor_ValidArguments_CreateInstanceSuccessfully()
    {
        // Arrange
        HouseNumber house = new("41");
        City city = new("Москва");
        Street street = new("Тверская");
        
        // Act
        var address = new Address(city, street, house);
        
        // Assert
        Assert.Equal(city, address.City);
        Assert.Equal(house, address.House);
        Assert.Equal(street, address.Street);
    }
    
    [Fact]
    public void Ctor_StreetIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        HouseNumber house = new("41");
        City city = new("Москва");
        
        // Act
        var act = () => new Address(city, null!, house);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("street", exception.ParamName);
    }

    [Fact]
    public void Ctor_WithCityIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        HouseNumber house = new("41");
        Street street = new("Тверская");
        
        // Act
        var act = () => new Address(null!, street, house);
        
        // Assert
        var ex = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("city", ex.ParamName);
    }

    [Fact]
    public void ToString_ReturnsCityAndStreetSeparateByComma()
    {
        // Arrange
        const string houseName = "41";
        const string cityName = "Москва";
        const string streetName = "Тверская";
        const string expected = $"{cityName}, {streetName}, {houseName}";
        
        City city = new(cityName);
        Street street = new(streetName);
        HouseNumber house = new(houseName);

        var address = new Address(city, street, house);
        
        // Act
        var result = address.ToString();
        
        // Assert
        Assert.Equal(expected, result);
    }
}