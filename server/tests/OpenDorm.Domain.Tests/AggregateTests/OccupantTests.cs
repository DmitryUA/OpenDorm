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
        const Gender gender = Gender.Male;

        // Act
        var occupant = new Occupant(id, lastName, firstName, patronymic, gender, birthDate);
        
        // Assert
        Assert.Equal(id, occupant.Id);
        Assert.Equal(lastName, occupant.LastName);
        Assert.Equal(firstName, occupant.FirstName);
        Assert.Equal(patronymic, occupant.Patronymic);
        Assert.Equal(fullName, occupant.FullName);
        Assert.Equal(gender, occupant.Gender);
        Assert.Equal(birthDate, occupant.BirthDate);
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
        const Gender gender = Gender.Male;

        // Act
        var occupant = new Occupant(id, lastName, firstName, null, gender, birthDate);
        
        // Assert
        Assert.Equal(id, occupant.Id);
        Assert.Equal(lastName, occupant.LastName);
        Assert.Equal(firstName, occupant.FirstName);
        Assert.Null(occupant.Patronymic);
        Assert.Equal(gender, occupant.Gender);
        Assert.Equal(birthDate, occupant.BirthDate);
    }

    [Fact]
    public void Constructor_NullLastName_ThrowsArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        const Gender gender = Gender.Male;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentNullException>(() =>
                new Occupant(id, null!, firstName, patronymic, gender, birthDate));
        
        Assert.Equal("lastName", exception.ParamName);
    }
    
    [Fact]
    public void Constructor_NullFirstName_ThrowsArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 01, 01));
        const Gender gender = Gender.Male;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentNullException>(() =>
                new Occupant(id, lastName, null!, patronymic, gender, birthDate));
        
        Assert.Equal("firstName", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullBirthDate_ThrowsArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        const Gender gender = Gender.Male;

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentNullException>(() =>
                new Occupant(id, lastName, firstName, patronymic, gender, null!));
        
        Assert.Equal("birthDate", exception.ParamName);
    }
    
    #endregion

    #region CheckIn Tests

    [Fact]
    public void CheckIn_ValidRoomId_OccupantCheckedInEvent()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var occupant = new OccupantBuilder().Build();
        
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
        var occupant = new OccupantBuilder().Build();
        occupant.CheckIn(Guid.NewGuid());
        
        // Act & Assert
        Assert.Throws<DomainException>(() => occupant.CheckIn(Guid.NewGuid()));
    }

    [Fact]
    public void CheckIn_InactiveOccupant_ThrowsDomainException()
    {
        // Arrange
        var occupant = new OccupantBuilder().Build();
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
        var occupant = new OccupantBuilder().Build();
        var roomId = Guid.NewGuid();
        occupant.CheckIn(roomId);
        
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
        var occupant = new OccupantBuilder().Build();
        
        // Act
        occupant.Activate();
        
        // Assert
        Assert.True(occupant.IsActive);
    }

    [Fact]
    public void Activate_Inactive_IsActiveIsTrue()
    {
        // Arrange
        var occupant = new OccupantBuilder().Build();
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
        var occupant = new OccupantBuilder().Build();
        
        // Act
        occupant.Deactivate();
        
        // Assert
        Assert.False(occupant.IsActive);
    }

    [Fact]
    public void Deactivate_Inactive_IsActiveIsFalse()
    {
        // Arrange
        var occupant = new OccupantBuilder().Build();
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
        var occupant = new OccupantBuilder().Build();
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
    
    private class OccupantBuilder
    {
        private readonly Guid _id = Guid.NewGuid();
        private LastName _lastName = new("Иванов");
        private FirstName _firstName = new("Иван");
        private Patronymic _patronymic = new("Иванович");
        private Gender _gender = Gender.Male;
        private BirthDate _birthDate = new(new DateOnly(2003, 01, 21));

        public OccupantBuilder WithLastName(LastName lastName)
        {
            _lastName = lastName;
            return this;
        }

        public OccupantBuilder WithFirstName(FirstName firstName)
        {
            _firstName = firstName;
            return this;
        }

        public OccupantBuilder WithPatronymic(Patronymic patronymic)
        {
            _patronymic = patronymic;
            return this;
        }

        public OccupantBuilder WithGender(Gender gender)
        {
            _gender = gender;
            return this;
        }

        public OccupantBuilder WithBirthDate(BirthDate birthDate)
        {
            _birthDate = birthDate;
            return this;
        }

        public Occupant Build() => new(_id, _lastName, _firstName, _patronymic, _gender, _birthDate);
    }
}