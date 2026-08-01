using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

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