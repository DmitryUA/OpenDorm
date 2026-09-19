using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;
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

namespace OpenDorm.Application.Tests.Features.Occupants.Commands;

public class CreateAccommodationCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private IOccupantRepository _repository = null!;
    private CreateAccommodationCommandHandler _handler = null!;
    
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

        var unitOfWork = new UnitOfWork(_context);
        _repository = new OccupantRepository(_context);
        _handler = new CreateAccommodationCommandHandler(_context, _repository, unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_StoreDataToDatabase()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        var occupant = OccupantFactory.Create();

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CreateAccommodationCommand(roomId, occupant.Id);
        var minTime = DateTime.UtcNow;
        var maxTime = minTime.AddSeconds(5);
        
        // Act
        var accommodationId = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        var loadedOccupant = await _repository.GetByIdAsync(occupant.Id);
        
        Assert.NotEmpty(loadedOccupant!.Accommodations);

        var accommodation = loadedOccupant.Accommodations.First();
        
        Assert.NotNull(accommodation);
        Assert.Equal(accommodationId, accommodation.Id);
        Assert.Equal(roomId, accommodation.RoomId);
        Assert.Null(accommodation.CheckOutDate);
        Assert.InRange(accommodation.CheckInDate, minTime, maxTime);
    }

    [Fact]
    public async Task Handle_NonExistRoom_ThrowsNotFoundException()
    {
        // Arrange
        var occupant = OccupantFactory.Create();

        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        
        var nonExistRoomId = Guid.NewGuid();
        var command = new CreateAccommodationCommand(nonExistRoomId, occupant.Id);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(nonExistRoomId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var nonExistOccupantId = Guid.NewGuid();
        var command = new CreateAccommodationCommand(roomId, nonExistOccupantId);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistOccupantId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_InactiveRoom_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        dormitory.DeactivateRoom(roomId);

        var occupant = OccupantFactory.Create();
        
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CreateAccommodationCommand(roomId, occupant.Id);
        var expectedMessage = $"Room is inactive. Room id: '{roomId}'.";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task Handle_FullyOccupiedRoom_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom(capacity: 1);
        var firstOccupant = OccupantFactory.Create();
        var secondOccupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        
        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(firstOccupant);
        await _context.OccupantsDbSet.AddAsync(secondOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CreateAccommodationCommand(roomId, firstOccupant.Id);
        var expectedMessage = $"There are no empty seats in the room. Room id: '{roomId}'.";

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