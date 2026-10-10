using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class RefreshTokenRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IUserRepository _userRepository = null!;

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
        _refreshTokenRepository = new RefreshTokenRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistRefreshToken()
    {
        // Arrange
        var user = UserFactory.Create();
        var token = RefreshTokenFactory.Create(userId: user.Id, token: "unique_token_123");

        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        // Act
        await _refreshTokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _refreshTokenRepository.GetByTokenAsync("unique_token_123");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(token.Id, loaded.Id);
        Assert.Equal(user.Id, loaded.UserId);
        Assert.Equal("unique_token_123", loaded.Token);
        Assert.False(loaded.IsRevoked);
    }

    #endregion

    #region GetByTokenAsync

    [Fact]
    public async Task GetByTokenAsync_ExistingToken_ReturnsRefreshToken()
    {
        // Arrange
        var user = UserFactory.Create();
        var token = RefreshTokenFactory.Create(user.Id, "findable_token");
        
        await _userRepository.AddAsync(user);
        await _refreshTokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _refreshTokenRepository.GetByTokenAsync("findable_token");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(token.Id, loaded.Id);
        Assert.Equal("findable_token", loaded.Token);
    }

    [Fact]
    public async Task GetByTokenAsync_NonExistingToken_ReturnsNull()
    {
        // Act
        var loaded = await _refreshTokenRepository.GetByTokenAsync("nonexistent_token");

        // Assert
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetByTokenAsync_RevokedToken_StillReturnsToken()
    {
        // Arrange
        var user = UserFactory.Create();
        var token = RefreshTokenFactory.Create(user.Id, "revoked_token");
        
        await _userRepository.AddAsync(user);
        await _refreshTokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _refreshTokenRepository.GetByTokenAsync("revoked_token");
        await _refreshTokenRepository.RevokeAsync(loaded!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _refreshTokenRepository.GetByTokenAsync("revoked_token");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRevoked);
    }

    [Fact]
    public async Task GetByTokenAsync_ExpiredToken_StillReturnsToken()
    {
        // Arrange
        var user = UserFactory.Create();
        var token = RefreshTokenFactory.CreateExpired(user.Id, "expired_token");
        
        await _userRepository.AddAsync(user);
        await _refreshTokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _refreshTokenRepository.GetByTokenAsync("expired_token");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(token.Id, loaded.Id);
    }

    #endregion

    #region RevokeAsync

    [Fact]
    public async Task RevokeAsync_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var user = UserFactory.Create();
        var token = RefreshTokenFactory.Create(user.Id, "to_revoke");
        
        await _userRepository.AddAsync(user);
        await _refreshTokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _refreshTokenRepository.GetByTokenAsync("to_revoke");

        // Act
        await _refreshTokenRepository.RevokeAsync(loaded!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _refreshTokenRepository.GetByTokenAsync("to_revoke");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRevoked);
    }

    #endregion

    #region DeleteExpiredAsync

    [Fact]
    public async Task DeleteExpiredAsync_ShouldRemoveOnlyExpiredTokens()
    {
        // Arrange
        var user1 = UserFactory.Create();
        var user2 = UserFactory.Create();
        var user3 = UserFactory.Create();
        var user4 = UserFactory.Create();
        
        var expired1 = RefreshTokenFactory.CreateExpired(user1.Id, "expired_1");
        var expired2 = RefreshTokenFactory.CreateExpired(user2.Id, "expired_2");
        var valid1 = RefreshTokenFactory.Create(user3.Id, "valid_1");
        var valid2 = RefreshTokenFactory.Create(user4.Id, "valid_2");

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.AddAsync(user3);
        await _userRepository.AddAsync(user4);
        await _refreshTokenRepository.AddAsync(expired1);
        await _refreshTokenRepository.AddAsync(expired2);
        await _refreshTokenRepository.AddAsync(valid1);
        await _refreshTokenRepository.AddAsync(valid2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _refreshTokenRepository.DeleteExpiredAsync();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var expired1Loaded = await _refreshTokenRepository.GetByTokenAsync("expired_1");
        var expired2Loaded = await _refreshTokenRepository.GetByTokenAsync("expired_2");
        var valid1Loaded = await _refreshTokenRepository.GetByTokenAsync("valid_1");
        var valid2Loaded = await _refreshTokenRepository.GetByTokenAsync("valid_2");

        Assert.Null(expired1Loaded);
        Assert.Null(expired2Loaded);
        Assert.NotNull(valid1Loaded);
        Assert.NotNull(valid2Loaded);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenNoExpiredTokens_ShouldNotRemoveAnything()
    {
        // Arrange
        var user1 = UserFactory.Create();
        var user2 = UserFactory.Create();
        var valid1 = RefreshTokenFactory.Create(user1.Id, "valid_only_1");
        var valid2 = RefreshTokenFactory.Create(user2.Id, "valid_only_2");

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _refreshTokenRepository.AddAsync(valid1);
        await _refreshTokenRepository.AddAsync(valid2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _refreshTokenRepository.DeleteExpiredAsync();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var valid1Loaded = await _refreshTokenRepository.GetByTokenAsync("valid_only_1");
        var valid2Loaded = await _refreshTokenRepository.GetByTokenAsync("valid_only_2");

        Assert.NotNull(valid1Loaded);
        Assert.NotNull(valid2Loaded);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenAllTokensExpired_ShouldRemoveAll()
    {
        // Arrange
        var user1 = UserFactory.Create();
        var user2 = UserFactory.Create();
        var expired1 = RefreshTokenFactory.CreateExpired(user1.Id, "all_expired_1");
        var expired2 = RefreshTokenFactory.CreateExpired(user2.Id,"all_expired_2");

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _refreshTokenRepository.AddAsync(expired1);
        await _refreshTokenRepository.AddAsync(expired2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _refreshTokenRepository.DeleteExpiredAsync();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var expired1Loaded = await _refreshTokenRepository.GetByTokenAsync("all_expired_1");
        var expired2Loaded = await _refreshTokenRepository.GetByTokenAsync("all_expired_2");

        Assert.Null(expired1Loaded);
        Assert.Null(expired2Loaded);
    }

    #endregion
}