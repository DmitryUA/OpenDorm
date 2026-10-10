using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Commands.CreateUser;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Integration;

public class CreateUserCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _userRepository = null!;
    private CreateUserCommandHandler _handler = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();
        var options = new DbContextOptionsBuilder<OpenDormDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        var encryptionService = new AesGcmEncryptionService(new byte[32]);
        _context = new OpenDormDbContext(options, encryptionService);
        await _context.Database.MigrateAsync();

        var uow = new UnitOfWork(_context);
        _userRepository = new UserRepository(_context);
        var passwordHasher = new BcryptPasswordHasher();

        _handler = new CreateUserCommandHandler(
            _context,
            _userRepository,
            passwordHasher,
            uow);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region Valid Command

    [Theory]
    [InlineData("Иванович")]
    [InlineData(null)]
    public async Task Handle_ValidCommand_StoresDataToDatabase(string? patronymic)
    {
        // Arrange
        const string login = "ivanov1984";
        const string password = "SecurePassword123!";
        const string lastName = "Иванов";
        const string firstName = "Иван";
        const string roleName = "Administrator";

        var command = new CreateUserCommand(
            login, password, lastName, firstName, patronymic, roleName);

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var user = await _userRepository.GetByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(login, user.Login.Value);
        Assert.Equal(lastName, user.LastName.Value);
        Assert.Equal(firstName, user.FirstName.Value);

        if (patronymic is null)
            Assert.Null(user.Patronymic);
        else
            Assert.Equal(patronymic, user.Patronymic!.Value);

        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPassword()
    {
        // Arrange
        const string plainPassword = "SecurePassword123!";
        var command = new CreateUserCommand(
            "ivanov1984",
            plainPassword,
            "Иванов",
            "Иван",
            "Иванович",
            "Administrator");

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var user = await _userRepository.GetByIdAsync(userId);
        Assert.NotNull(user);
        Assert.NotEqual(plainPassword, user.PasswordHash.Value);
        Assert.StartsWith("$2a$12$", user.PasswordHash.Value);

        var passwordHasher = new BcryptPasswordHasher();
        Assert.True(passwordHasher.Verify(plainPassword, user.PasswordHash));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUserId()
    {
        // Arrange
        var command = new CreateUserCommand(
            "ivanov1984",
            "SecurePassword123!",
            "Иванов",
            "Иван",
            "Иванович",
            "Administrator");

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, userId);
    }

    #endregion

    #region Role Assignment

    [Fact]
    public async Task Handle_ValidCommand_AssignsAdministratorRole()
    {
        // Arrange
        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new CreateUserCommand(
            "ivanov1984",
            "SecurePassword123!",
            "Иванов",
            "Иван",
            "Иванович",
            "Administrator");

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var user = await _userRepository.GetByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Single(user.UserRoles);
        Assert.Contains(user.UserRoles, ur => ur.RoleId == administratorRoleId);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsManagerRole()
    {
        // Arrange
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var command = new CreateUserCommand(
            "manager_user",
            "SecurePassword123!",
            "Петров",
            "Пётр",
            null,
            "Manager");

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var user = await _userRepository.GetByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Single(user.UserRoles);
        Assert.Contains(user.UserRoles, ur => ur.RoleId == managerRoleId);
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    [InlineData("Administrator")]
    [InlineData("AdMiNiStRaToR")]
    public async Task Handle_RoleNameInAnyCase_AssignsCorrectRole(string roleName)
    {
        // Arrange
        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new CreateUserCommand(
            $"user_{roleName.GetHashCode()}",
            "SecurePassword123!",
            "Петров",
            "Пётр",
            null,
            roleName);

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var user = await _userRepository.GetByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Single(user.UserRoles);
        Assert.Contains(user.UserRoles, ur => ur.RoleId == administratorRoleId);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Handle_ExistingLogin_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateUserCommand(
            "ivanov1984",
            "SecurePassword123!",
            "Иванов",
            "Иван",
            "Иванович",
            "Administrator");

        await _handler.Handle(command, CancellationToken.None);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("ivanov1984", exception.Message);
    }

    [Fact]
    public async Task Handle_NonExistingRole_ThrowsNotFoundException()
    {
        // Arrange
        var command = new CreateUserCommand(
            "new-user",
            "SecurePassword123!",
            "Иванов",
            "Иван",
            "Иванович",
            "SuperAdmin");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.Role), exception.EntityName);
        Assert.Equal("SuperAdmin", exception.EntityId);
    }

    #endregion
}