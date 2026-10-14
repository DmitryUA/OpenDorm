using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Users.Queries.GetUserList;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Users.Queries.Integration;

public class GetUserListQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _userRepository = null!;
    private GetUserListQueryHandler _handler = null!;

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
        _handler = new GetUserListQueryHandler(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region Empty list

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetUserListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Sorting

    [Fact]
    public async Task Handle_MultipleUsers_SortsByLastNameThenFirstName()
    {
        // Arrange
        var user1 = UserFactory.Create(
            login: new Login("user1"),
            lastName: new LastName("Петров"),
            firstName: new FirstName("Иван"),
            patronymic: null);
        var user2 = UserFactory.Create(
            login: new Login("user2"),
            lastName: new LastName("Иванов"),
            firstName: new FirstName("Пётр"),
            patronymic: null);
        var user3 = UserFactory.Create(
            login: new Login("user3"),
            lastName: new LastName("Иванов"),
            firstName: new FirstName("Алексей"),
            patronymic: null);
        var user4 = UserFactory.Create(
            login: new Login("user4"),
            lastName: new LastName("Сидоров"),
            firstName: new FirstName("Иван"),
            patronymic: null);

        // Добавляем в произвольном порядке
        await _userRepository.AddAsync(user3);
        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user4);
        await _userRepository.AddAsync(user2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal("Иванов", result[0].LastName);
        Assert.Equal("Алексей", result[0].FirstName);
        Assert.Equal("Иванов", result[1].LastName);
        Assert.Equal("Пётр", result[1].FirstName);
        Assert.Equal("Петров", result[2].LastName);
        Assert.Equal("Иван", result[2].FirstName);
        Assert.Equal("Сидоров", result[3].LastName);
        Assert.Equal("Иван", result[3].FirstName);
    }

    #endregion

    #region Roles mapping

    [Fact]
    public async Task Handle_UserWithoutRoles_ReturnsDtoWithEmptyRoles()
    {
        // Arrange
        var user = UserFactory.Create();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Empty(result[0].Roles);
    }

    [Fact]
    public async Task Handle_UserWithOneRole_ReturnsDtoWithNormalizedRoleName()
    {
        // Arrange
        var user = UserFactory.Create();
        user.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Administrator
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Roles);
        Assert.Contains("ADMINISTRATOR", result[0].Roles);
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

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Roles.Count);
        Assert.Contains("ADMINISTRATOR", result[0].Roles);
        Assert.Contains("MANAGER", result[0].Roles);
    }

    #endregion

    #region Patronymic handling

    [Fact]
    public async Task Handle_UserWithPatronymic_MapsPatronymicCorrectly()
    {
        // Arrange
        var user = UserFactory.Create(patronymic: new Patronymic("Иванович"));
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Иванович", result[0].Patronymic);
    }

    [Fact]
    public async Task Handle_UserWithoutPatronymic_MapsPatronymicAsNull()
    {
        // Arrange
        var user = UserFactory.CreateWithoutPatronymic();
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Patronymic);
    }

    #endregion

    #region Full mapping

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

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        var dto = result[0];
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

    [Fact]
    public async Task Handle_MultipleUsers_MapsAllFieldsCorrectly()
    {
        // Arrange
        var user1 = UserFactory.Create(
            login: new Login("user1"),
            lastName: new LastName("Иванов"),
            firstName: new FirstName("Иван"),
            patronymic: new Patronymic("Иванович"));
        user1.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Administrator

        var user2 = UserFactory.CreateWithoutPatronymic(
            login: new Login("user2"),
            lastName: new LastName("Петров"),
            firstName: new FirstName("Пётр"));
        user2.AssignRole(Guid.Parse("00000000-0000-0000-0000-000000000002")); // Manager

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetUserListQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        // Первый — Иванов (сортировка по LastName)
        Assert.Equal(user1.Id, result[0].Id);
        Assert.Equal("user1", result[0].Login);
        Assert.Equal("Иванов", result[0].LastName);
        Assert.Equal("Иван", result[0].FirstName);
        Assert.Equal("Иванович", result[0].Patronymic);
        Assert.Contains("ADMINISTRATOR", result[0].Roles);

        // Второй — Петров
        Assert.Equal(user2.Id, result[1].Id);
        Assert.Equal("user2", result[1].Login);
        Assert.Equal("Петров", result[1].LastName);
        Assert.Equal("Пётр", result[1].FirstName);
        Assert.Null(result[1].Patronymic);
        Assert.Contains("MANAGER", result[1].Roles);
    }

    #endregion
}