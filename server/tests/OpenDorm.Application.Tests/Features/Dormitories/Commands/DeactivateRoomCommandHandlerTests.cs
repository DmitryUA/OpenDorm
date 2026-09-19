using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Commands.DeactivateRoom;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands;

public class DeactivateRoomCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private IUnitOfWork _unitOfWork = null!;
    private OpenDormDbContext _context = null!;
    private IDormitoryRepository _repository = null!;
    private DeactivateRoomCommandHandler _handler = null!;
    
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

        _unitOfWork = new UnitOfWork(_context);
        _repository = new DormitoryRepository(_context);
        _handler = new DeactivateRoomCommandHandler(_repository, _context, _unitOfWork);
    }

    [Fact]
    public async Task Handle_UnoccupiedRoom_MakeRoomInactive()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();

        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync(CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new DeactivateRoomCommand(dormitory.Id, roomId);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        var roomStatus = _context.Rooms.First(r => r.Id == roomId).IsActive;
        Assert.False(roomStatus);
    }

    [Fact]
    public async Task Handle_OccupiedRoom_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync(CancellationToken.None);

        var (occupant, accommodationId) = OccupantFactory.CreateCheckedIn(roomId);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        _context.ChangeTracker.Clear();

        var expectedMessage =
            $"It is impossible to deactivate a room that is currently occupied. Accommodation id: '{accommodationId}'";
        var command = new DeactivateRoomCommand(dormitory.Id, roomId);
        
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