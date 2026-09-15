using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Commands.ActivateRoom;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands;

public class ActivateRoomCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private IUnitOfWork _unitOfWork = null!;
    private OpenDormDbContext _context = null!;
    private IDormitoryRepository _repository = null!;
    private ActivateRoomCommandHandler _handler = null!;
    
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
        _handler = new ActivateRoomCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_InactiveRoom_MakeRoomInactive()
    {
        // Arrange
        var (dormitory, roomsIds) = DormitoryFactory.CreateWithRooms(roomsCount: 1);
        var roomId = roomsIds[0];
        
        dormitory.DeactivateRoom(roomId);

        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync(CancellationToken.None);
        _context.ChangeTracker.Clear();

        var command = new ActivateRoomCommand(dormitory.Id, roomId);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        var roomStatus = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.Id == roomId)
            .Select(r => r.IsActive)
            .FirstAsync(CancellationToken.None);
        
        Assert.True(roomStatus);
    }

    [Fact]
    public async Task Handle_NonExistedDormitory_ThrowsNotFoundException()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        var command = new ActivateRoomCommand(dormitoryId, Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Dormitory), exception.EntityName);
        Assert.Equal(dormitoryId, exception.EntityId);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}