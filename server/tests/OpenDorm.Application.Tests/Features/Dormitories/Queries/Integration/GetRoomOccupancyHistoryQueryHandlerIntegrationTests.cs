using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.Integration;

public class GetRoomOccupancyHistoryQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetRoomOccupancyHistoryQueryHandler _handler = null!;
    
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

        _handler = new GetRoomOccupancyHistoryQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsRoomHistorySortedInDescendingOrder()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom(capacity: 2);
        var firstOccupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        var (lastOccupant, lastAccommodationId) = OccupantFactory.CreateCheckedIn(roomId);
        
        firstOccupant.CheckOut();

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(firstOccupant);
        await _context.OccupantsDbSet.AddAsync(lastOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetRoomOccupancyHistoryQuery(roomId);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(query.Page, result.Page);
        Assert.Equal(query.PageSize, result.PageSize);
        
        var firstHistoryItem = result.Items[0];
        
        Assert.Equal(lastOccupant.Id, firstHistoryItem.OccupantId);
        Assert.Equal(lastAccommodationId, firstHistoryItem.AccommodationId);
        
        Assert.Equal(lastOccupant.LastName.Value, firstHistoryItem.LastName);
        Assert.Equal(lastOccupant.FirstName.Value, firstHistoryItem.FirstName);
        Assert.Equal(lastOccupant.Patronymic?.Value, firstHistoryItem.Patronymic);

        Assert.Equal(lastOccupant.Gender, firstHistoryItem.Gender);
        Assert.Equal(lastOccupant.BirthDate.Value, firstHistoryItem.BirthDate);

        var lastAccommodation = lastOccupant.Accommodations.First();
        var timeDiffMs = Math.Abs((firstHistoryItem.CheckInDate - lastAccommodation.CheckInDate).TotalMilliseconds);
        
        Assert.True(timeDiffMs < 1, $"Разница во времени составляет {timeDiffMs} мс, ожидалось < 1 мс");
        Assert.Null(firstHistoryItem.CheckOutDate);

        var lastHistoryItem = result.Items[1];
        Assert.NotNull(lastHistoryItem.CheckOutDate);
        Assert.True(firstHistoryItem.CheckInDate > lastHistoryItem.CheckInDate);
    }

    [Theory]
    [InlineData(SortOrder.Ascending)]
    [InlineData(SortOrder.Descending)]
    public async Task Handle_Sort_ReturnsSortedResult(SortOrder sortOrder)
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom(capacity: 2);
        var firstOccupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        var lastOccupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(firstOccupant);
        await _context.OccupantsDbSet.AddAsync(lastOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetRoomOccupancyHistoryQuery(roomId, sortOrder);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        switch (sortOrder)
        {
            case SortOrder.Ascending:
                Assert.True(result.Items[0].CheckInDate < result.Items[1].CheckInDate);
                break;
            case SortOrder.Descending:
                Assert.True(result.Items[0].CheckInDate > result.Items[1].CheckInDate);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null);
        }
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}