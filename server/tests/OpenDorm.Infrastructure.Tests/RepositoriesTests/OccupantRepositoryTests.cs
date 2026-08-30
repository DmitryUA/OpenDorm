using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Infrastructure.Tests.RepositoriesTests;

public class OccupantRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private OccupantRepository _occupantRepository = null!;
    private DormitoryRepository _dormitoryRepository = null!;

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
        
        _occupantRepository = new OccupantRepository(_context);
        _dormitoryRepository = new DormitoryRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistOccupant()
    {
        // Arrange
        const Gender gender = Gender.Male;
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 1, 21));
        
        var occupant = new Occupant(Guid.NewGuid(), lastName, firstName, patronymic, gender, birthDate);
        
        // Act
        await _occupantRepository.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _occupantRepository.GetByIdAsync(occupant.Id);
        
        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(gender, loaded.Gender);
        Assert.Equal(lastName, loaded.LastName);
        Assert.Equal(firstName, loaded.FirstName);
        Assert.Equal(patronymic, loaded.Patronymic);
        Assert.Equal(birthDate, loaded.BirthDate);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistOccupantWithAccommodation()
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
        
        await _dormitoryRepository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        const Gender gender = Gender.Male;
        var lastName = new LastName("Иванов");
        var firstName = new FirstName("Иван");
        var patronymic = new Patronymic("Иванович");
        var birthDate = new BirthDate(new DateOnly(2003, 1, 21));
        
        var occupant = new Occupant(Guid.NewGuid(), lastName, firstName, patronymic, gender, birthDate);
        
        var accommodationId = occupant.CheckIn(roomId);
        
        // Act
        await _occupantRepository.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loadedOccupant = await _context.OccupantsDbSet
            .FirstOrDefaultAsync(o => o.Id == occupant.Id);

        var loadedAccommodation = await _context.Set<Accommodation>()
            .FirstOrDefaultAsync(a => a.Id == accommodationId);
        
        // Assert
        Assert.NotNull(loadedOccupant);
        Assert.NotNull(loadedAccommodation);
        Assert.Null(loadedAccommodation.CheckOutDate);
        Assert.Equal(roomId, loadedAccommodation.RoomId);
        Assert.Equal(DateTime.Now.Date, loadedAccommodation.CheckInDate.Date);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOccupantNotFound()
    {
        // Act
        var loaded = await _occupantRepository.GetByIdAsync(Guid.NewGuid());
        
        // Assert
        Assert.Null(loaded);
    }
}