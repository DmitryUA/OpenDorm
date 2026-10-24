using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Domain.Tests.AggregateTests;

public class OccupantTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidParams_CreateInstanceAndPreservesValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var fullName = $"{lastName} {firstName} {patronymic}";
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        var phone = new PhoneNumber("+7 (999) 123-45-67");
        const Gender gender = Gender.Male;

        // Act
        var occupant = new Occupant(id, lastName, firstName, patronymic, gender, birthDate, phone);
        
        // Assert
        Assert.Equal(id, occupant.Id);
        Assert.Equal(lastName, occupant.LastName);
        Assert.Equal(firstName, occupant.FirstName);
        Assert.Equal(patronymic, occupant.Patronymic);
        Assert.Equal(fullName, occupant.FullName);
        Assert.Equal(gender, occupant.Gender);
        Assert.Equal(birthDate, occupant.BirthDate);
        Assert.Equal(phone, occupant.Phone);
        Assert.True(occupant.IsActive);
    }

    [Fact]
    public void Constructor_NullPatronymic_CreateInstanceAndPreservesValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        var phone = new PhoneNumber("+7 (999) 123-45-67");
        const Gender gender = Gender.Male;

        // Act
        var occupant = new Occupant(id, lastName, firstName, null, gender, birthDate, phone);
        
        // Assert
        Assert.Equal(id, occupant.Id);
        Assert.Equal(lastName, occupant.LastName);
        Assert.Equal(firstName, occupant.FirstName);
        Assert.Null(occupant.Patronymic);
        Assert.Equal(gender, occupant.Gender);
        Assert.Equal(birthDate, occupant.BirthDate);
        Assert.Equal(phone, occupant.Phone);
    }

    [Fact]
    public void Constructor_NullLastName_ThrowsInvalidOccupantException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        const Gender gender = Gender.Male;
        const string expectedMessage = "Occupant last name cannot be null.";

        // Act & Assert
        var exception =
            Assert.Throws<InvalidOccupantException>(() =>
                new Occupant(id, null!, firstName, patronymic, gender, birthDate, new PhoneNumber("+79991234567")));
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void Constructor_NullFirstName_ThrowsInvalidOccupantException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        const Gender gender = Gender.Male;
        const string expectedMessage = "Occupant first name cannot be null.";

        // Act & Assert
        var exception =
            Assert.Throws<InvalidOccupantException>(() =>
                new Occupant(id, lastName, null!, patronymic, gender, birthDate, new PhoneNumber("+79991234567")));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NullBirthDate_ThrowsInvalidOccupantException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        const Gender gender = Gender.Male;
        const string expectedMessage = "Occupant birth date cannot be null.";

        // Act & Assert
        var exception =
            Assert.Throws<InvalidOccupantException>(() =>
                new Occupant(id, lastName, firstName, patronymic, gender, null!, new PhoneNumber("+79991234567")));
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    #endregion

    #region CheckIn Tests

    [Fact]
    public void CheckIn_ValidRoomId_OccupantCheckedInEvent()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var occupant = OccupantFactory.Create();
        
        // Act
        occupant.CheckIn(roomId);
        
        // Assert
        var checkedInEvent = Assert.IsType<OccupantCheckedInEvent>(occupant.DomainEvents.Last());
        Assert.Equal(roomId, checkedInEvent.RoomId);
        Assert.Equal(occupant.Id, checkedInEvent.OccupantId);
    }

    [Fact]
    public void CheckIn_OccupantIsAlreadyLivingInAnotherRoom_ThrowsDomainException()
    {
        // Arrange
        var occupant = OccupantFactory.CreateCheckedIn(Guid.NewGuid()).Occupant;
        var expectedMessage =
            $"Occupant '{occupant.FullName}' is already living in another room. Occupant id: '{occupant.Id}'.";
        
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => occupant.CheckIn(Guid.NewGuid()));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void CheckIn_InactiveOccupant_ThrowsDomainException()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        occupant.Deactivate();
        var expectedMessage = $"Inactive occupant cannot be moved in. Occupant id: '{occupant.Id}'.";
        
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => occupant.CheckIn(Guid.NewGuid()));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region CheckOut Tests

    [Fact]
    public void CheckOut_OccupantWithActiveAccommodation_OccupantCheckedOutEventWillBeCreated()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var occupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        
        // Act
        occupant.CheckOut();
        
        // Assert
        var checkOutEvent = Assert.IsType<OccupantCheckedOutEvent>(occupant.DomainEvents.Last());
        Assert.Equal(roomId, checkOutEvent.RoomId);
        Assert.Equal(occupant.Id, checkOutEvent.OccupantId);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_Active_IsActiveIsTrue()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        
        // Act
        occupant.Activate();
        
        // Assert
        Assert.True(occupant.IsActive);
    }

    [Fact]
    public void Activate_Inactive_IsActiveIsTrue()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        occupant.Deactivate();
        
        // Act
        occupant.Activate();
        
        // Assert
        Assert.True(occupant.IsActive);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_Active_IsActiveIsFalse()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        
        // Act
        occupant.Deactivate();
        
        // Assert
        Assert.False(occupant.IsActive);
    }

    [Fact]
    public void Deactivate_Inactive_IsActiveIsFalse()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        occupant.Deactivate();
        
        // Act
        occupant.Deactivate();
        
        // Assert
        Assert.False(occupant.IsActive);
    }

    [Fact]
    public void Deactivate_ActiveAccommodation_ThrowsDomainException()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        occupant.CheckIn(Guid.NewGuid());
        var expectedMessage =
            $"Cannot deactivate occupant '{occupant.FullName}' with active accommodation. Occupant id: '{occupant.Id}'.";
        
        // Act & Assert
        var exception = Assert.Throws<DomainException>(occupant.Deactivate);
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region Transfer Tests

    [Fact]
    public void Transfer_UnoccupiedRoom_TransferOccupant()
    {
        // Arrange
        var roomToId = Guid.NewGuid();
        var roomFromId = Guid.NewGuid();
        var (occupant, firsAccommodationId) = OccupantFactory.CreateCheckedIn(roomFromId);
        
        // Act
        var lastAccommodationId = occupant.Transfer(roomToId);
        
        // Assert
        var firstAccommodation = occupant.Accommodations.First(a => a.Id == firsAccommodationId);
        var lastAccommodation = occupant.Accommodations.FirstOrDefault(a => a.Id == lastAccommodationId);
        
        Assert.NotNull(firstAccommodation.CheckOutDate);
        Assert.NotNull(lastAccommodation);
        Assert.Null(lastAccommodation.CheckOutDate);
        Assert.Equal(roomToId ,lastAccommodation.RoomId);
    }

    [Fact]
    public void Transfer_RoomWhereThisOccupantIsAlreadyLiving_ThrowsDomainException()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var (occupant, accommodationId) = OccupantFactory.CreateCheckedIn(roomId);
        var expectedMessage = $"Occupant is already living in this room. Accommodation id: '{accommodationId}'.";
        
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => occupant.Transfer(roomId));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region ChangeLastName Tests

    [Fact]
    public void ChangeLastName_ValidLastName_SaveValue()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        var newLastName = new LastName("Шариков");
        
        // Act
        occupant.ChangeLastName(newLastName);
        
        // Assert
        Assert.Equal(newLastName, occupant.LastName);
    }

    [Fact]
    public void ChangeLastName_Null_ThrowsInvalidOccupantException()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        const string expectedMessage = "Occupant last name cannot be null.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOccupantException>(() => occupant.ChangeLastName(null!));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region ChangeFirstName Tests

    [Fact]
    public void ChangeFirstName_ValidFirstName_SaveValue()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        var newFirstName = new FirstName("Аркадий");
        
        // Act
        occupant.ChangeFirstName(newFirstName);
        
        // Assert
        Assert.Equal(newFirstName, occupant.FirstName);
    }

    [Fact]
    public void ChangeFirstName_Null_ThrowsInvalidOccupantException()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        const string expectedMessage = "Occupant first name cannot be null.";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOccupantException>(() => occupant.ChangeFirstName(null!));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region ChangePatronymic Tests

    [Fact]
    public void ChangePatronymic_ValidPatronymic_SaveValue()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        var newPatronymic = new Patronymic("Аркадьевич");
        
        // Act
        occupant.ChangePatronymic(newPatronymic);
        
        // Assert
        Assert.Equal(newPatronymic, occupant.Patronymic);
    }

    #endregion

    #region Phone Tests

    [Fact]
    public void Constructor_NullPhone_ThrowsInvalidOccupantException()
    {
        var exception = Assert.Throws<InvalidOccupantException>(() =>
            new Occupant(
                Guid.NewGuid(),
                new LastName("Иванов"),
                new FirstName("Иван"),
                null,
                Gender.Male,
                new BirthDate(new DateOnly(2003, 1, 1)),
                null!));

        Assert.Equal("Occupant phone cannot be null.", exception.Message);
    }

    [Fact]
    public void ChangePhone_ValidPhone_SavesValue()
    {
        var occupant = OccupantFactory.Create();
        var phone = new PhoneNumber("8 (999) 123-45-67");

        occupant.ChangePhone(phone);

        Assert.Equal("+79991234567", occupant.Phone.Value);
    }

    [Fact]
    public void ChangePhone_Null_ThrowsInvalidOccupantException()
    {
        var occupant = OccupantFactory.Create();

        var exception = Assert.Throws<InvalidOccupantException>(() => occupant.ChangePhone(null!));

        Assert.Equal("Occupant phone cannot be null.", exception.Message);
    }

    #endregion
}