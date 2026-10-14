using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Queries.GetUserById;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Queries.Integration;

public class GetUserByIdQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _userRepository = null!;
    private GetUserByIdQueryHandler _handler = null!;

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

        _userRepository = new UserRepository(_context);
        _handler = new GetUserByIdQueryHandler(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region User not found

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetUserByIdQuery(nonExistingId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal(nameof(User), exception.EntityName);
        Assert.Equal(nonExistingId, exception.EntityId);
    }

    #endregion

    #region User without roles

    [Fact]
    public async Task Handle_UserWithoutRoles_ReturnsDtoWithEmptyRoles()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Login.Value, dto.Login);
        Assert.Equal(user.LastName.Value, dto.LastName);
        Assert.Equal(user.FirstName.Value, dto.FirstName);
        Assert.Equal(user.Patronymic?.Value, dto.Patronymic);
        Assert.Equal(user.IsActive, dto.IsActive);
        Assert.Empty(dto.Roles);
    }

    #endregion

    #region User with roles

    [Fact]
    public async Task Handle_UserWithOneRole_ReturnsDtoWithNormalizedRoleName()
    {
        // Arrange
        var user = UserFactory.Create();
        user.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Administrator
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(dto.Roles);
        Assert.Contains("ADMINISTRATOR", dto.Roles);
    }

    [Fact]
    public async Task Handle_UserWithMultipleRoles_ReturnsDtoWithAllNormalizedRoleNames()
    {
        // Arrange
        var user = UserFactory.Create();
        user.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Administrator
        user.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000002")); // Manager
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, dto.Roles.Count);
        Assert.Contains("ADMINISTRATOR", dto.Roles);
        Assert.Contains("MANAGER", dto.Roles);
    }

    #endregion

    #region Patronymic handling

    [Fact]
    public async Task Handle_UserWithPatronymic_ReturnsDtoWithPatronymic()
    {
        // Arrange
        var user = UserFactory.Create(patronymic: new Patronymic("Иванович"));
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal("Иванович", dto.Patronymic);
    }

    [Fact]
    public async Task Handle_UserWithoutPatronymic_ReturnsDtoWithNullPatronymic()
    {
        // Arrange
        var user = UserFactory.CreateWithoutPatronymic();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(dto.Patronymic);
    }

    #endregion

    #region All fields mapping

    [Fact]
    public async Task Handle_ValidUser_MapsAllFieldsCorrectly()
    {
        // Arrange
        var user = UserFactory.Create(
            login: new Login("test-user"),
            lastName: new LastName("Петров"),
            firstName: new FirstName("Пётр"),
            patronymic: new Patronymic("Петрович"));
        user.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000002")); // Manager
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var dto = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal("test-user", dto.Login);
        Assert.Equal("Петров", dto.LastName);
        Assert.Equal("Пётр", dto.FirstName);
        Assert.Equal("Петрович", dto.Patronymic);
        Assert.True(dto.IsActive);
        Assert.NotEqual(default, dto.CreatedAt);
        Assert.Single(dto.Roles);
        Assert.Contains("MANAGER", dto.Roles);
    }

    #endregion
}