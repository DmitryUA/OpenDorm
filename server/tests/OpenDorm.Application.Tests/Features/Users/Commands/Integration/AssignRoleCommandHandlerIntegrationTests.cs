using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Commands.AssignRole;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Integration;

public class AssignRoleCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _userRepository = null!;
    private AssignRoleCommandHandler _handler = null!;

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
        _handler = new AssignRoleCommandHandler(_context, _userRepository, uow);
    }

    #region Valid Role Assignment

    [Fact]
    public async Task Handle_ValidCommand_AssignsAdministratorRole()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new AssignRoleCommand(user.Id, "Administrator");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == administratorRoleId);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsManagerRole()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var command = new AssignRoleCommand(user.Id, "Manager");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == managerRoleId);
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    [InlineData("Administrator")]
    [InlineData("AdMiNiStRaToR")]
    public async Task Handle_RoleNameInAnyCase_AssignsCorrectRole(string roleName)
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new AssignRoleCommand(user.Id, roleName);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == administratorRoleId);
    }

    [Fact]
    public async Task Handle_MultipleRoles_AssignsBothRoles()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var command1 = new AssignRoleCommand(user.Id, "Administrator");
        var command2 = new AssignRoleCommand(user.Id, "Manager");

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.UserRoles.Count);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == administratorRoleId);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == managerRoleId);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var command = new AssignRoleCommand(Guid.NewGuid(), "Administrator");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.User), exception.EntityName);
    }

    [Fact]
    public async Task Handle_NonExistingRole_ThrowsNotFoundException()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new AssignRoleCommand(user.Id, "SuperAdmin");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.Role), exception.EntityName);
        Assert.Equal("SuperAdmin", exception.EntityId);
    }

    [Fact]
    public async Task Handle_DuplicateRoleAssignment_ThrowsDomainException()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new AssignRoleCommand(user.Id, "Administrator");

        // 1-е назначение роли (успешное)
        await _handler.Handle(command, CancellationToken.None);

        // !!! КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ !!!
        // Очищаем трекер, чтобы сымитировать новый HTTP-запрос (новый scope DbContext).
        // Это заставит второй вызов GetByIdAsync реально загрузить User с его UserRoles из БД,
        // и доменная проверка _userRoles.Any(...) корректно сработает.
        _context.ChangeTracker.Clear();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
    
        Assert.Contains("already has this role", exception.Message);
    }

    #endregion
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}