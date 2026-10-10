using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Enums;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class RoleRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private RoleRepository _repository = null!;

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
        _repository = new RoleRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRole()
    {
        // Arrange
        var administratorId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var loaded = await _repository.GetByIdAsync(administratorId);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(administratorId, loaded.Id);
        Assert.Equal(RoleName.Administrator, loaded.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var loaded = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(loaded);
    }

    #endregion

    #region GetByNameAsync

    [Fact]
    public async Task GetByNameAsync_ExistingName_ReturnsRole()
    {
        // Act
        var loaded = await _repository.GetByNameAsync(RoleName.Manager);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(RoleName.Manager, loaded.Name);
        Assert.Equal("MANAGER", loaded.NormalizedName);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingName_ReturnsNull()
    {
        // Act
        var loaded = await _repository.GetByNameAsync((RoleName)999);

        // Assert
        Assert.Null(loaded);
    }

    #endregion
}