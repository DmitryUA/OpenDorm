using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class RoomNameTests
{
    [Fact]
    public void Ctor_WithValidName_CreateInstanceAndPreservesName()
    {
        // Arrange
        const string name = "406";
        
        // Act
        var roomName = new RoomName(name);
        
        // Assert
        Assert.Equal(name, roomName.Name);
    }

    [Fact]
    public void Ctor_WithLeadingAndTrailingWhitespace_TrimsName()
    {
        // Arrange
        const string name = "406  ";
        const string expectedName = "406";
        
        // Act
        var roomName = new RoomName(name);
        
        // Assert
        Assert.Equal(expectedName, roomName.Name);
    }

    [Fact]
    public void Ctor_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        const string? nullName = null;
        
        // Act
        var act = () => new RoomName(nullName!);
        
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_WithEmptyOrWhitespace_ThrowsArgumentException(string name)
    {
        // Arrange & Act
        var act = () => new RoomName(name);
        
        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Ctor_WithLengthGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var longName = new string('A', RoomName.MaxNameLength + 1);
        
        // Act
        var act = () => new RoomName(longName);
        
        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Ctor_WithMinValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('A', RoomName.MinNameLength);
        
        // Act
        var roomName = new RoomName(minValidName);
        
        // Assert
        Assert.Equal(minValidName, roomName.Name);
    }

    [Fact]
    public void Ctor_WithMaxValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('A', RoomName.MaxNameLength);
        
        // Act
        var roomName = new RoomName(maxValidName);
        
        // Assert
        Assert.Equal(maxValidName, roomName.Name);
    }
}