using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+7 (999) 123-45-67", "+79991234567")]
    [InlineData("8 999 123-45-67", "+79991234567")]
    [InlineData("9991234567", "+79991234567")]
    public void Constructor_ValidPhone_NormalizesToInternationalFormat(string input, string expected)
    {
        var phone = new PhoneNumber(input);

        Assert.Equal(expected, phone.Value);
        Assert.Equal(expected, phone.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("+1 999 123 4567")]
    [InlineData("+7 999 123 45 678")]
    public void Constructor_InvalidPhone_ThrowsInvalidPhoneNumber(string input)
    {
        Assert.Throws<InvalidPhoneNumber>(() => new PhoneNumber(input));
    }

    [Fact]
    public void Constructor_NullPhone_ThrowsInvalidPhoneNumber()
    {
        Assert.Throws<InvalidPhoneNumber>(() => new PhoneNumber(null!));
    }
}
