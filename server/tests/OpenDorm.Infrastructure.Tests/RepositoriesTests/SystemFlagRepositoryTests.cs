using Microsoft.EntityFrameworkCore;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class SystemFlagRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private SystemFlagRepository _repository = null!;

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
        _repository = new SystemFlagRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistSystemFlag()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(
            name: "feature:dark-mode",
            value: "enabled");

        // Act
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _context.SystemFlagsDbSet
            .FirstOrDefaultAsync(f => f.Id == flag.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(flag.Id, loaded.Id);
        Assert.Equal("feature:dark-mode", loaded.Name);
        Assert.Equal("enabled", loaded.Value);
    }

    [Fact]
    public async Task AddAsync_ShouldPreserveCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var flag = SystemFlagFactory.Create();

        // Act
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _context.SystemFlagsDbSet
            .FirstOrDefaultAsync(f => f.Id == flag.Id);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(loaded);
        Assert.InRange(loaded.CreatedAt, before, after);
    }

    [Fact]
    public async Task AddAsync_ShouldPreserveValueWithoutTrimming()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(
            name: "  flag-with-spaces  ",
            value: "  value-with-spaces  ");

        // Act
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _context.SystemFlagsDbSet
            .FirstOrDefaultAsync(f => f.Id == flag.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("  flag-with-spaces  ", loaded.Name);
        Assert.Equal("  value-with-spaces  ", loaded.Value);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(name: "feature:dark-mode");
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByNameAsync("feature:dark-mode");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsByNameAsync("nonexistent-flag");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_PartialMatch_ReturnsFalse()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(name: "feature:dark-mode");
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByNameAsync("feature:dark");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_DifferentCase_ReturnsFalse()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(name: "feature:dark-mode");
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByNameAsync("FEATURE:DARK-MODE");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_AfterAdd_ReturnsTrue()
    {
        // Arrange
        var flag = SystemFlagFactory.Create(name: "maintenance-mode");

        // Act
        await _repository.AddAsync(flag);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        var exists = await _repository.ExistsByNameAsync("maintenance-mode");

        // Assert
        Assert.True(exists);
    }

    #endregion
}