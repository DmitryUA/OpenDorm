using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetailsById;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.GetDormitoryDetailsById;

public class GetDormitoryDetailsByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetDormitoryDetailsByIdQueryHandler _handler = null!;
    
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
        
        _handler = new GetDormitoryDetailsByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_DormitoryWithoutRooms_ReturnsStoredDetails()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        var city = new City("Москва");
        var street = new Street("Строителей");
        var house = new HouseNumber("12");
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(dormitoryId, address);

        _context.DormitoriesDbSet.Add(dormitory);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _handler.Handle(
            new GetDormitoryDetailsByIdQuery(dormitoryId),
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(dormitoryId, result.Id);
        Assert.Equal(city.Value, result.City);
        Assert.Equal(street.Value, result.Street);
        Assert.Equal(house.Value, result.House);
        Assert.Equal(1, result.TotalFloorCount);
        Assert.Equal(0, result.TotalRoomCount);
        Assert.Equal(0, result.TotalSeatCount);
        Assert.Equal(0, result.TotalAvailablePlaceCount);
    }

    [Fact]
    public async Task Handle_DormitoryWithUnoccupiedRooms_ReturnsStoredDetails()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        var city = new City("Москва");
        var street = new Street("Строителей");
        var house = new HouseNumber("12");
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(dormitoryId, address, 2);

        dormitory.AddRoom(new RoomName("109"), Gender.Female, 2);
        dormitory.AddRoom(new RoomName("206"), Gender.Male, 2, 2);

        _context.DormitoriesDbSet.Add(dormitory);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _handler.Handle(
            new GetDormitoryDetailsByIdQuery(dormitoryId),
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(dormitoryId, result.Id);
        Assert.Equal(city.Value, result.City);
        Assert.Equal(street.Value, result.Street);
        Assert.Equal(house.Value, result.House);
        Assert.Equal(2, result.TotalFloorCount);
        Assert.Equal(2, result.TotalRoomCount);
        Assert.Equal(4, result.TotalSeatCount);
        Assert.Equal(4, result.TotalAvailablePlaceCount);
    }

    [Fact]
    public async Task Handle_DormitoryWithOccupiedRooms()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        var city = new City("Москва");
        var street = new Street("Строителей");
        var house = new HouseNumber("12");
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(dormitoryId, address, 2);

        var firstRoomId = dormitory.AddRoom(new RoomName("109"), Gender.Male, 2);
        var secondRoomId = dormitory.AddRoom(new RoomName("206"), Gender.Female, 2, 2);

        var firstOccupant = new Occupant(Guid.NewGuid(),
            new LastName("Иванов"),
            new FirstName("Иван"),
            new Patronymic("Иванович"),
            Gender.Male,
            new BirthDate(new DateOnly(2003, 01, 21)));
        
        var secondOccupant = new Occupant(Guid.NewGuid(),
            new LastName("Иванова"),
            new FirstName("Мария"),
            new Patronymic("Александровна"),
            Gender.Female,
            new BirthDate(new DateOnly(2003, 10, 12)));

        // TODO:
        // Заменить прямой вызов CheckIn на использование команды на заселение
        // например 'CheckInOccupantCommand' (на 2026-09-02 ещё не создана).
        firstOccupant.CheckIn(firstRoomId);
        secondOccupant.CheckIn(secondRoomId);

        _context.DormitoriesDbSet.Add(dormitory);
        _context.OccupantsDbSet.AddRange(firstOccupant, secondOccupant);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _handler.Handle(
            new GetDormitoryDetailsByIdQuery(dormitoryId),
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(dormitoryId, result.Id);
        Assert.Equal(city.Value, result.City);
        Assert.Equal(street.Value, result.Street);
        Assert.Equal(house.Value, result.House);
        Assert.Equal(2, result.TotalFloorCount);
        Assert.Equal(2, result.TotalRoomCount);
        Assert.Equal(4, result.TotalSeatCount);
        Assert.Equal(2, result.TotalAvailablePlaceCount);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();
}