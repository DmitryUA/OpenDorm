using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Tests.ValueObjectsTests;

public class RoomNameTests
{
    [Fact]
    public void Ctor_ValidValue_CreateInstanceAndPreservesValue()
    {
        // Arrange
        const string name = "406";
        
        // Act
        var roomName = new RoomName(name);
        
        // Assert
        Assert.Equal(name, roomName.Value);
    }

    [Fact]
    public void Ctor_LeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string name = "406  ";
        const string expectedName = "406";
        
        // Act
        var roomName = new RoomName(name);
        
        // Assert
        Assert.Equal(expectedName, roomName.Value);
    }
    
    [Fact]
    public void Ctor_MaxValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var maxValidName = new string('A', RoomName.MaxLength);
        
        // Act
        var roomName = new RoomName(maxValidName);
        
        // Assert
        Assert.Equal(maxValidName, roomName.Value);
    }
    
    [Fact]
    public void Ctor_MinValidLength_CreateInstanceSuccessfully()
    {
        // Arrange
        var minValidName = new string('A', RoomName.MinLength);
        
        // Act
        var roomName = new RoomName(minValidName);
        
        // Assert
        Assert.Equal(minValidName, roomName.Value);
    }

    [Fact]
    public void Ctor_Null_ThrowsInvalidRoomNameException()
    {
        // Arrange
        const string expectedMessage = "Room name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidRoomNameException>(() => new RoomName(null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Ctor_EmptyOrWhitespace_ThrowsInvalidRoomNameException(string name)
    {
        // Arrange
        const string expectedMessage = "Room name cannot be empty.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidRoomNameException>(() => new RoomName(name));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Ctor_LengthGreaterThanMax_ThrowsInvalidRoomNameException()
    {
        // Arrange
        var longName = new string('A', RoomName.MaxLength + 1);
        var expectedMessage = $"Room name cannot be exceed {RoomName.MaxLength} characters.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidRoomNameException>(() => new RoomName(longName));
        Assert.Equal(expectedMessage, exception.Message);
    }
}