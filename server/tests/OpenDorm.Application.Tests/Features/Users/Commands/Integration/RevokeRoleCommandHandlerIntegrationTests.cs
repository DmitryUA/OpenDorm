using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Commands.AssignRole;
using OpenDorm.Application.Features.Users.Commands.RevokeRole;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Integration;

public class RevokeRoleCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _userRepository = null!;
    private AssignRoleCommandHandler _assignRoleHandler = null!;
    private RevokeRoleCommandHandler _revokeRoleHandler = null!;

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
        _assignRoleHandler = new AssignRoleCommandHandler(_context, _userRepository, uow);
        _revokeRoleHandler = new RevokeRoleCommandHandler(_context, _userRepository, uow);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region Successful Revocation

    [Fact]
    public async Task Handle_ExistingRole_RemovesRoleFromUser()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _assignRoleHandler.Handle(
            new AssignRoleCommand(user.Id, "Administrator"),
            CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, "Administrator");

        // Act
        await _revokeRoleHandler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Empty(loaded.UserRoles);
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    [InlineData("Administrator")]
    [InlineData("AdMiNiStRaToR")]
    public async Task Handle_RoleNameInAnyCase_RemovesRoleSuccessfully(string roleName)
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _assignRoleHandler.Handle(
            new AssignRoleCommand(user.Id, "Administrator"),
            CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, roleName);

        // Act
        await _revokeRoleHandler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Empty(loaded.UserRoles);
    }

    [Fact]
    public async Task Handle_WhenUserHasMultipleRoles_RemovesOnlySpecifiedRole()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _assignRoleHandler.Handle(
            new AssignRoleCommand(user.Id, "Administrator"),
            CancellationToken.None);
        await _assignRoleHandler.Handle(
            new AssignRoleCommand(user.Id, "Manager"),
            CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, "Administrator");

        // Act
        await _revokeRoleHandler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == managerRoleId);
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var command = new RevokeRoleCommand(Guid.NewGuid(), "Administrator");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _revokeRoleHandler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.User), exception.EntityName);
    }

    [Fact]
    public async Task Handle_InvalidRoleName_ThrowsNotFoundException()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, "SuperAdmin");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _revokeRoleHandler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.Role), exception.EntityName);
        Assert.Equal("SuperAdmin", exception.EntityId);
    }

    [Fact]
    public async Task Handle_RoleNotAssignedToUser_ThrowsDomainException()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, "Administrator");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _revokeRoleHandler.Handle(command, CancellationToken.None));
        Assert.Contains("does not have this role", exception.Message);
    }

    [Fact]
    public async Task Handle_SameRoleTwice_ThrowsDomainExceptionOnSecondCall()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _assignRoleHandler.Handle(
            new AssignRoleCommand(user.Id, "Administrator"),
            CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new RevokeRoleCommand(user.Id, "Administrator");
        await _revokeRoleHandler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _revokeRoleHandler.Handle(command, CancellationToken.None));
        Assert.Contains("does not have this role", exception.Message);
    }

    #endregion
}