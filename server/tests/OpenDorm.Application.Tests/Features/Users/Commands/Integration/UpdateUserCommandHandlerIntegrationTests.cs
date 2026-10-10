using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Commands.UpdateUser;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Integration;

public class UpdateUserCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _repository = null!;
    private UpdateUserCommandHandler _handler = null!;

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
        var passwordHasher = new BcryptPasswordHasher();

        _handler = new UpdateUserCommandHandler(_repository, passwordHasher, uow);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region User Not Found

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: null,
            NewPassword: null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Domain.Aggregates.User.User), exception.EntityName);
    }

    #endregion

    #region Personal Data Update

    [Fact]
    public async Task Handle_UpdatesPersonalData_PersistsToDatabase()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: "Петрович",
            NewPassword: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName.Value);
        Assert.Equal("Пётр", loaded.FirstName.Value);
        Assert.Equal("Петрович", loaded.Patronymic!.Value);
    }

    [Fact]
    public async Task Handle_UpdatesPersonalDataWithoutPatronymic_SetsPatronymicToNull()
    {
        // Arrange
        var user = UserFactory.Create(); // с отчеством
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: null,
            NewPassword: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName.Value);
        Assert.Equal("Пётр", loaded.FirstName.Value);
        Assert.Null(loaded.Patronymic);
    }

    #endregion

    #region Password Change

    [Fact]
    public async Task Handle_ChangesPassword_HashesAndPersistsToDatabase()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        const string newPassword = "NewSecurePassword456!";
        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: null,
            Patronymic: null,
            NewPassword: newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.NotEqual(newPassword, loaded.PasswordHash.Value);
        Assert.StartsWith("$2a$12$", loaded.PasswordHash.Value);

        var passwordHasher = new BcryptPasswordHasher();
        Assert.True(passwordHasher.Verify(newPassword, loaded.PasswordHash));
    }

    #endregion

    #region Combined Updates

    [Fact]
    public async Task Handle_UpdatesBothPersonalDataAndPassword_PersistsBothToDatabase()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        const string newPassword = "NewSecurePassword456!";
        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: "Петрович",
            NewPassword: newPassword);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName.Value);
        Assert.Equal("Пётр", loaded.FirstName.Value);
        Assert.Equal("Петрович", loaded.Patronymic!.Value);

        var passwordHasher = new BcryptPasswordHasher();
        Assert.True(passwordHasher.Verify(newPassword, loaded.PasswordHash));
    }

    #endregion

    #region No-Op Update

    [Fact]
    public async Task Handle_NoFieldsProvided_DoesNotChangeDataButStillSaves()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var originalLastName = user.LastName.Value;
        var originalFirstName = user.FirstName.Value;
        var originalHash = user.PasswordHash;

        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: null,
            Patronymic: null,
            NewPassword: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(loaded);
        Assert.Equal(originalLastName, loaded.LastName.Value);
        Assert.Equal(originalFirstName, loaded.FirstName.Value);
        Assert.Equal(originalHash, loaded.PasswordHash);
    }

    #endregion
}