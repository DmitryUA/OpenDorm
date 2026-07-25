using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Tests.EntityTests;

public class AccomodationTests
{
    #region ConstructorTests

    [Fact]
    public void Constructor_ValidArguments_CreateInstanceAndPreservesValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var checkInDate = DateTime.UtcNow;
        
        // Act
        var accomodation = new Accommodation(id, roomId, checkInDate);
        
        // Assert
        Assert.Equal(id, accomodation.Id);
        Assert.Equal(roomId, accomodation.RoomId);
        Assert.Equal(checkInDate, accomodation.CheckInDate);
        Assert.Null(accomodation.CheckOutDate);
        Assert.True(accomodation.IsActive);
    }

    [Fact]
    public void Constructor_EmptyRoomId_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var checkInDate = DateTime.UtcNow;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Accommodation(id, Guid.Empty, checkInDate));
        Assert.Equal("roomId", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyCheckInDate_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Accommodation(id, roomId, default));
        Assert.Equal("checkInDate", exception.ParamName);
    }

    [Fact]
    public void Constructor_FutureDate_ThrowsDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var checkInDate = DateTime.UtcNow.AddDays(1);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => new Accommodation(id, roomId, checkInDate));
    }

    #endregion

    #region CheckOut Tests

    [Fact]
    public void CheckOut_ValidValue_CheckOutPreservesValueAndIsActiveSetFalse()
    {
        // Arrange
        var accomodation = new Accommodation(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        var checkOutDate = DateTime.UtcNow;
        // Act
        accomodation.CheckOut(checkOutDate);
        
        // Assert
        Assert.Equal(checkOutDate, accomodation.CheckOutDate);
        Assert.False(accomodation.IsActive);
    }
    
    [Fact]
    public void CheckOut_AccommodationIsAlreadyInactive_ThrowsDomainException()
    {
        // Arrange
        var accomodation = new Accommodation(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        accomodation.CheckOut(DateTime.UtcNow);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => accomodation.CheckOut(DateTime.UtcNow));
    }

    [Fact]
    public void CheckOut_CheckOutDateIsEarlierThanCheckInDate_ThrowsDomainException()
    {
        // Arrange
        var checkInDate = DateTime.UtcNow;
        var checkOutDate = checkInDate.AddDays(-1);
        var accomodation = new Accommodation(Guid.NewGuid(), Guid.NewGuid(), checkInDate);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => accomodation.CheckOut(checkOutDate));
    }
    
    #endregion
}