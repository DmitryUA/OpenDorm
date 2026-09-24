using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.Integration;

public class GetRoomOccupantsQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetRoomOccupantsQueryHandler _handler = null!;
    
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

        _handler = new GetRoomOccupantsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_RoomWithOneOccupant_ReturnsCollectionsWithOneItem()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        var occupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetRoomOccupantsQuery(roomId);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Single(result);

        var loadedOccupant = result.First();
        Assert.Equal(occupant.Id, loadedOccupant.Id);
        Assert.Equal(occupant.LastName.Value, loadedOccupant.LastName);
        Assert.Equal(occupant.FirstName.Value, loadedOccupant.FirstName);
        Assert.Equal(occupant.Patronymic?.Value, loadedOccupant.Patronymic);
        Assert.Equal(occupant.BirthDate.Value, loadedOccupant.BirthDate);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}