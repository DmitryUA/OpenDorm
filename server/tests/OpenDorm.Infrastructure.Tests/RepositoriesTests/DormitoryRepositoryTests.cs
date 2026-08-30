using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class DormitoryRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private DormitoryRepository _repository = null!;

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

        _repository = new DormitoryRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
    
    [Fact]
    public async Task AddAsync_ShouldPersistDormitory()
    {
        // Arrange
        var address = new Address(new City("Москва"), new Street("Строителей"), new HouseNumber("12"));
        var dormitory = new Dormitory(
            Guid.NewGuid(),
            address,
            floorCount: 5);

        // Act
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(dormitory.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(address, loaded.Address);
        Assert.Equal(5, loaded.FloorCount);
    }
    
    [Fact]
    public async Task AddAsync_ShouldPersistDormitoryWithRooms()
    {
        // Arrange
        var dormitory = new Dormitory(
            Guid.NewGuid(),
            new Address(new City("Москва"), new Street("Строителей"), new HouseNumber("12")),
            floorCount: 5);

        var roomId = dormitory.AddRoom(
            new RoomName("101"),
            Gender.Male,
            capacity: 2,
            floorNumber: 1);

        // Act
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var loadedDormitory = await _context.DormitoriesDbSet
            .FirstOrDefaultAsync(d => d.Id == dormitory.Id);
    
        var loadedRoom = await _context.Set<Room>()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        // Assert
        Assert.NotNull(loadedDormitory);
        Assert.NotNull(loadedRoom);
        Assert.Equal("101", loadedRoom.Name.Value);
        Assert.Equal(Gender.Male, loadedRoom.Gender);
        Assert.Equal(2, loadedRoom.Capacity);
        Assert.Equal(1, loadedRoom.FloorNumber);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDormitoryNotFound()
    {
        // Act
        var loaded = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(loaded);
    }
}