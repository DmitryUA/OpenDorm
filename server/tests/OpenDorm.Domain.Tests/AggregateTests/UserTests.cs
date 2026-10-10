using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Domain.Tests.AggregateTests;

public class UserTests
{
    #region Constructor

    [Fact]
    public void Constructor_ValidArgumentsWithPatronymic_CreatesInstanceAndPreservesValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var login = new Login("ivanov1984");
        var passwordHash = new PasswordHash("$2a$12$hash");
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");

        // Act
        var user = new User(id, login, passwordHash, lastName, firstName, patronymic);

        // Assert
        Assert.Equal(id, user.Id);
        Assert.Equal(login, user.Login);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(patronymic, user.Patronymic);
    }

    [Fact]
    public void Constructor_ValidArgumentsWithoutPatronymic_CreatesInstanceWithNullPatronymic()
    {
        // Arrange
        var id = Guid.NewGuid();
        var login = new Login("ivanov1984");
        var passwordHash = new PasswordHash("$2a$12$hash");
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");

        // Act
        var user = new User(id, login, passwordHash, lastName, firstName, null);

        // Assert
        Assert.Null(user.Patronymic);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsCreatedAtCloseToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var user = UserFactory.Create();

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(user.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsIsActiveToTrue()
    {
        // Act
        var user = UserFactory.Create();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Constructor_ValidArgumentsWithPatronymic_FormatsFullNameCorrectly()
    {
        // Arrange
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var user = UserFactory.Create(lastName: lastName, firstName: firstName, patronymic: patronymic);

        // Assert
        Assert.Equal("Иванов Иван Иванович", user.FullName);
    }

    [Fact]
    public void Constructor_ValidArgumentsWithoutPatronymic_FormatsFullNameWithoutPatronymic()
    {
        // Arrange
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var user = UserFactory.CreateWithoutPatronymic(lastName: lastName, firstName: firstName);

        // Assert
        Assert.Equal("Иванов Иван", user.FullName);
    }

    [Fact]
    public void Constructor_NullLogin_ThrowsInvalidUserException()
    {
        // Arrange
        const string expectedMessage = "Login cannot be null.";
        var passwordHash = new PasswordHash("$2a$12$hash");
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            new User(Guid.NewGuid(), null!, passwordHash, lastName, firstName, null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NullPasswordHash_ThrowsInvalidUserException()
    {
        // Arrange
        const string expectedMessage = "Password hash cannot be null.";
        var login = new Login("ivanov1984");
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            new User(Guid.NewGuid(), login, null!, lastName, firstName, null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NullLastName_ThrowsInvalidUserException()
    {
        // Arrange
        const string expectedMessage = "Last name cannot be null.";
        var login = new Login("ivanov1984");
        var passwordHash = new PasswordHash("$2a$12$hash");
        var firstName = new FirstName("Иван");

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            new User(Guid.NewGuid(), login, passwordHash, null!, firstName, null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NullFirstName_ThrowsInvalidUserException()
    {
        // Arrange
        const string expectedMessage = "First name cannot be null.";
        var login = new Login("ivanov1984");
        var passwordHash = new PasswordHash("$2a$12$hash");
        var lastName = new LastName("Иванов");

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            new User(Guid.NewGuid(), login, passwordHash, lastName, null!, null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region UpdatePersonalData

    [Fact]
    public void UpdatePersonalData_ValidArgumentsWithPatronymic_UpdatesProperties()
    {
        // Arrange
        var user = UserFactory.Create();
        var newLastName = new LastName("Петров");
        var newFirstName = new FirstName("Пётр");
        var newPatronymic = new Patronymic("Петрович");

        // Act
        user.UpdatePersonalData(newLastName, newFirstName, newPatronymic);

        // Assert
        Assert.Equal(newLastName, user.LastName);
        Assert.Equal(newFirstName, user.FirstName);
        Assert.Equal(newPatronymic, user.Patronymic);
    }

    [Fact]
    public void UpdatePersonalData_ValidArgumentsWithoutPatronymic_UpdatesPropertiesAndSetsNullPatronymic()
    {
        // Arrange
        var user = UserFactory.Create();
        var newLastName = new LastName("Петров");
        var newFirstName = new FirstName("Пётр");

        // Act
        user.UpdatePersonalData(newLastName, newFirstName, null);

        // Assert
        Assert.Equal(newLastName, user.LastName);
        Assert.Equal(newFirstName, user.FirstName);
        Assert.Null(user.Patronymic);
    }

    [Fact]
    public void UpdatePersonalData_NullLastName_ThrowsInvalidUserException()
    {
        // Arrange
        var user = UserFactory.Create();
        const string expectedMessage = "Last name cannot be null.";

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            user.UpdatePersonalData(null!, new FirstName("Иван"), null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void UpdatePersonalData_NullFirstName_ThrowsInvalidUserException()
    {
        // Arrange
        var user = UserFactory.Create();
        const string expectedMessage = "First name cannot be null.";

        // Act & Assert
        var exception = Assert.Throws<InvalidUserException>(() =>
            user.UpdatePersonalData(new LastName("Иванов"), null!, null));
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region AssignRole

    [Fact]
    public void AssignRole_NewRoleId_AddsUserRoleToCollection()
    {
        // Arrange
        var user = UserFactory.Create();
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId);

        // Assert
        Assert.Single(user.UserRoles);
        Assert.Contains(user.UserRoles, ur => ur.RoleId == roleId && ur.UserId == user.Id);
    }

    [Fact]
    public void AssignRole_ExistingRoleId_ThrowsDomainException()
    {
        // Arrange
        var user = UserFactory.Create();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);
        var expectedMessage = $"User '{user.FullName}' already has this role. User id: '{user.Id}'.";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => user.AssignRole(roleId));
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region RevokeRole

    [Fact]
    public void RevokeRole_ExistingRoleId_RemovesUserRoleFromCollection()
    {
        // Arrange
        var user = UserFactory.Create();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        // Act
        user.RevokeRole(roleId);

        // Assert
        Assert.Empty(user.UserRoles);
    }

    [Fact]
    public void RevokeRole_NonExistingRoleId_ThrowsDomainException()
    {
        // Arrange
        var user = UserFactory.Create();
        var roleId = Guid.NewGuid();
        var expectedMessage = $"User '{user.FullName}' does not have this role. User id: '{user.Id}'.";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => user.RevokeRole(roleId));
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region ChangePassword

    [Fact]
    public void ChangePassword_ValidNewPasswordHash_UpdatesPasswordHash()
    {
        // Arrange
        var user = UserFactory.Create();
        var newPasswordHash = new PasswordHash("$2a$12$hash");

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_InactiveUser_SetsIsActiveToTrue()
    {
        // Arrange
        var user = UserFactory.CreateInactive();

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Activate_ActiveUser_DoesNotThrowAndKeepsIsActiveTrue()
    {
        // Arrange
        var user = UserFactory.Create();

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_ActiveUser_SetsIsActiveToFalse()
    {
        // Arrange
        var user = UserFactory.Create();

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_InactiveUser_DoesNotThrowAndKeepsIsActiveFalse()
    {
        // Arrange
        var user = UserFactory.CreateInactive();

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    #endregion
}