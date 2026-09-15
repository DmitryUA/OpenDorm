using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryRoomsList;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries;

public class GetDormitoryRoomsListQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private GetDormitoryRoomsListQueryHandler _handler = null!;
    
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

        _handler = new GetDormitoryRoomsListQueryHandler(_context);
    }
    
    [Fact]
    public async Task Handle_NoFilters_WillReturnListOfTwoItems()
    {
        // Arrange
        var (dormitory, roomsIds) = DormitoryFactory.CreateWithRooms();
        dormitory.DeactivateRoom(roomsIds[0]);

        await _context.DormitoriesDbSet.AddAsync(dormitory);

        var occupant = OccupantFactory.CreateWithHistory(roomsIds).Occupant;

        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetDormitoryRoomsListQuery(dormitory.Id, Page: 1, PageSize: 2);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(query.Page, result.Page);
        Assert.Equal(query.PageSize, result.PageSize);
        Assert.Equal(dormitory.Rooms.Count, result.TotalCount);
        Assert.Equal(dormitory.Rooms.Count, result.Items.Count);

        var activeRoom = dormitory.Rooms.First(r => r.IsActive);
        var activeRoomInfo = result.Items.First(r => r.Id == activeRoom.Id);
        
        Assert.True(activeRoomInfo.IsActive);
        Assert.Equal(1, activeRoomInfo.OccupantCount);
        Assert.Equal(activeRoom.Gender, activeRoomInfo.Gender);
        Assert.Equal(activeRoom.Name.Value, activeRoomInfo.Name);
        Assert.Equal(activeRoom.Capacity, activeRoomInfo.Capacity);
        Assert.Equal(activeRoom.FloorNumber, activeRoomInfo.FloorNumber);
    }

    [Fact]
    public async Task Handle_FilterByName_WillReturnListOfOneItem()
    {
        // Arrange
        var dormitory = DormitoryFactory.CreateWithRooms().Dormitory;

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var firstRoom = dormitory.Rooms.First();
        var query = new GetDormitoryRoomsListQuery(dormitory.Id, firstRoom.Name.Value, Page: 1, PageSize: 2);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(firstRoom.Id, result.Items.First().Id);
    }

    [Fact]
    public async Task Handle_FilterByStatus_WillReturnListOfOneObject()
    {
        // Arrange
        var (dormitory, roomsIds) = DormitoryFactory.CreateWithRooms();
        var inactiveRoomId = roomsIds[0];
        dormitory.DeactivateRoom(inactiveRoomId);
        
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetDormitoryRoomsListQuery(dormitory.Id, IsActive: false, Page: 1, PageSize: 2);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(inactiveRoomId, result.Items.First().Id);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}