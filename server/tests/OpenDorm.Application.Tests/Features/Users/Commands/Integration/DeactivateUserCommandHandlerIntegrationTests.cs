using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Commands.DeactivateUser;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Integration;

public class DeactivateUserCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _repository = null!;
    private DeactivateUserCommandHandler _handler = null!;

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
        _repository = new UserRepository(_context);
        _handler = new DeactivateUserCommandHandler(_repository, uow);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Handle_ActiveUser_SetsIsActiveToFalse()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new DeactivateUserCommand(user.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.False(loaded.IsActive);
    }

    [Fact]
    public async Task Handle_InactiveUser_KeepsIsActiveFalse()
    {
        // Arrange
        var user = UserFactory.CreateInactive();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new DeactivateUserCommand(user.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.False(loaded.IsActive);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.User), exception.EntityName);
    }
}