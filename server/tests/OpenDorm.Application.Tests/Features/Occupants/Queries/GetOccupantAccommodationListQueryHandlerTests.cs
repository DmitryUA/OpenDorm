using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Queries;

public class GetOccupantAccommodationListQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetOccupantAccommodationListQueryHandler _handler = null!;
    
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

        _handler = new GetOccupantAccommodationListQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_OccupantWithHistory_ReturnsOccupantAccommodationHistory()
    {
        // Assert
        var (dormitory, roomIds) = DormitoryFactory.CreateWithRooms(roomsCount: 2);
        var (occupant, accommodationsIds, activeAccommodationId) = OccupantFactory.CreateWithHistory(roomIds);
        
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var query = new GetOccupantAccommodationListQuery(occupant.Id);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.Count);

        var firstRoom = dormitory.Rooms.First(r => r.Id == roomIds[0]);
        var loadedFirstAccommodation = result.First(a => a.Id == accommodationsIds[0]);
        var firstAccommodation = occupant.Accommodations.First(a => a.Id == accommodationsIds[0]);

        Assert.Equal(firstRoom.Name.Value, loadedFirstAccommodation.Address.RoomName);
        Assert.Equal(dormitory.Address.ToString(), loadedFirstAccommodation.Address.DormitoryAddress);
        /*
         * Оссбенность в различиях типов: .NET DateTime имеет точность 7 знаков посля запятой,
         * PostgreSQL timestamp(tz) имеет точность 6 знаков после запятой,
         * из-за этого прямое сравнение дат заселения/выселения даст несовпадение.
        */
        var timeDiffMs = Math.Abs((loadedFirstAccommodation.CheckInDate - firstAccommodation.CheckInDate).TotalMilliseconds);
        Assert.True(timeDiffMs < 1, $"Разница во времени составляет {timeDiffMs} мс, ожидалось < 1 мс");
        
        // То же самое для CheckOutDate
        if (firstAccommodation.CheckOutDate.HasValue && loadedFirstAccommodation.CheckOutDate.HasValue)
        {
            var checkOutDiffMs = Math.Abs((loadedFirstAccommodation.CheckOutDate.Value - firstAccommodation.CheckOutDate.Value).TotalMilliseconds);
            Assert.True(checkOutDiffMs < 1);
        }
        else
            Assert.Equal(firstAccommodation.CheckOutDate, loadedFirstAccommodation.CheckOutDate);
    }

    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Arrange 
        var notExistOccupantId = Guid.NewGuid();
        var query = new GetOccupantAccommodationListQuery(notExistOccupantId);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(notExistOccupantId, exception.EntityId);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}