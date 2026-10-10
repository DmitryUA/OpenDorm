using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private UserRepository _repository = null!;

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
        _repository = new UserRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistUser()
    {
        // Arrange
        var user = UserFactory.Create();

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(user.Login, loaded.Login);
        Assert.Equal(user.PasswordHash, loaded.PasswordHash);
        Assert.Equal(user.LastName, loaded.LastName);
        Assert.Equal(user.FirstName, loaded.FirstName);
        Assert.Equal(user.Patronymic, loaded.Patronymic);
        Assert.Equal(user.IsActive, loaded.IsActive);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserWithRoles()
    {
        // Arrange
        var user = UserFactory.Create();
        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        user.AssignRole(administratorRoleId);
        user.AssignRole(managerRoleId);

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.UserRoles.Count);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == administratorRoleId);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == managerRoleId);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserWithoutPatronymic()
    {
        // Arrange
        var user = UserFactory.CreateWithoutPatronymic();

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Null(loaded.Patronymic);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        var user = UserFactory.Create();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var loaded = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesUserRoles()
    {
        // Arrange
        var user = UserFactory.Create();
        var roleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        user.AssignRole(roleId);
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == roleId);
    }

    #endregion

    #region GetByLoginAsync

    [Fact]
    public async Task GetByLoginAsync_ExistingLogin_ReturnsUser()
    {
        // Arrange
        var user = UserFactory.Create(login: new Login("ivanov1984"));
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByLoginAsync(new Login("ivanov1984"));

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded.Id);
        Assert.Equal("ivanov1984", loaded.Login.Value);
    }

    [Fact]
    public async Task GetByLoginAsync_NonExistingLogin_ReturnsNull()
    {
        // Act
        var loaded = await _repository.GetByLoginAsync(new Login("nonexistent"));

        // Assert
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetByLoginAsync_IncludesUserRoles()
    {
        // Arrange
        var user = UserFactory.Create(login: new Login("ivanov1984"));
        var roleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        user.AssignRole(roleId);
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByLoginAsync(new Login("ivanov1984"));

        // Assert
        Assert.NotNull(loaded);
        Assert.Single(loaded.UserRoles);
        Assert.Contains(loaded.UserRoles, ur => ur.RoleId == roleId);
    }

    #endregion

    #region ExistsByLoginAsync

    [Fact]
    public async Task ExistsByLoginAsync_ExistingLogin_ReturnsTrue()
    {
        // Arrange
        var user = UserFactory.Create(login: new Login("ivanov1984"));
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByLoginAsync(new Login("ivanov1984"));

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByLoginAsync_NonExistingLogin_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsByLoginAsync(new Login("nonexistent"));

        // Assert
        Assert.False(exists);
    }

    #endregion
}