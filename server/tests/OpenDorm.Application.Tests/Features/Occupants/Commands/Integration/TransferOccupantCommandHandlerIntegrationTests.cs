using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.TransferOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Integration;

public class TransferOccupantCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private IUnitOfWork _uow = null!;
    private OpenDormDbContext _context = null!;
    private IOccupantRepository _repository = null!;
    private TransferOccupantCommandHandler _handler = null!;
    
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

        _uow = new UnitOfWork(_context);
        _repository = new OccupantRepository(_context);
        _handler = new TransferOccupantCommandHandler(_uow, _context, _repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_TransferOccupant()
    {
        // Assert
        var (dormitory, roomIds) = DormitoryFactory.CreateWithRooms(roomsCount: 2);
        var (occupant, firstAccommodationId) = OccupantFactory.CreateCheckedIn(roomIds[0]);

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var minTime = DateTime.UtcNow;
        var maxTime = minTime.AddSeconds(5);
        var command = new TransferOccupantCommand(occupant.Id, roomIds[1]);
        
        // Act
        var lastAccommodationId = await _handler.Handle(command, CancellationToken.None);
        
        // Asser
        var lastAccommodation = await _context.Accommodations.FirstAsync(a => a.Id == lastAccommodationId);
        var firstAccommodation = await _context.Accommodations.FirstAsync(a => a.Id == firstAccommodationId);

        Assert.NotNull(firstAccommodation.CheckOutDate);
        Assert.InRange((DateTime)firstAccommodation.CheckOutDate, minTime, maxTime);

        Assert.NotNull(lastAccommodation);
        Assert.Null(lastAccommodation.CheckOutDate);
        Assert.InRange(lastAccommodation.CheckInDate, minTime, maxTime);
    }

    [Fact]
    public async Task Handle_NonExistTargetRoom_ThrowsNotFoundException()
    {
        // Assert
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        var occupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var nonExistRoomId = Guid.NewGuid();
        var command = new TransferOccupantCommand(occupant.Id, nonExistRoomId);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(nonExistRoomId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Assert
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var nonExistOccupantId = Guid.NewGuid();
        var command = new TransferOccupantCommand(nonExistOccupantId, roomId);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistOccupantId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_FullyOccupiedTargetRoom_ThrowsDomainException()
    {
        // Assert
        var (dormitory, roomIds) = DormitoryFactory.CreateWithRooms(roomsCount: 2, capacity: 1);
        var roomFromId = roomIds[0];
        var roomToId = roomIds[1];
        var occupantForTransfer = OccupantFactory.CreateCheckedIn(roomFromId).Occupant;
        var anotherOccupant = OccupantFactory.CreateCheckedIn(roomToId).Occupant;

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(anotherOccupant);
        await _context.OccupantsDbSet.AddAsync(occupantForTransfer);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new TransferOccupantCommand(occupantForTransfer.Id, roomToId);
        var expectedMessage = $"There are no empty seats in the room. Room id: '{roomToId}'.";
        
        // Act & Asser
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task Handle_InactiveTargetRoom_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomIds) = DormitoryFactory.CreateWithRooms(roomsCount: 2, capacity: 1);
        var roomFromId = roomIds[0];
        var roomToId = roomIds[1];
        var occupant = OccupantFactory.CreateCheckedIn(roomFromId).Occupant;
        
        dormitory.DeactivateRoom(roomToId);
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new TransferOccupantCommand(occupant.Id, roomToId);
        var expectedMessage = $"Room is inactive. Room id: '{roomToId}'.";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(expectedMessage, exception.Message);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}