using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Commands.CreateRoom;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands;

public class CreateRoomCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private IUnitOfWork _unitOfWork = null!;
    private OpenDormDbContext _context = null!;
    private IDormitoryRepository _repository = null!;
    private CreateRoomCommandHandler _handler = null!;
    
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
        
        _handler = new CreateRoomCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_WillPreserveRoom()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        var city = new City("Москва");
        var street = new Street("Строителей");
        var house = new HouseNumber("12");
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(dormitoryId, address, 9);

        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CreateRoomCommand(
            dormitoryId,
            "406",
            Gender.Male,
            2,
            4);
        
        // Act
        var roomId = await _handler.Handle(command, CancellationToken.None);
        var loadedDormitory = await _repository.GetByIdAsync(dormitoryId);
        var room = loadedDormitory!.Rooms.FirstOrDefault(r => r.Id == roomId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, roomId);
        Assert.NotNull(room);
        Assert.Equal(command.RoomName, room.Name.Value);
        Assert.Equal(command.Gender, room.Gender);
        Assert.Equal(command.Capacity, room.Capacity);
        Assert.Equal(command.FloorNumber, command.FloorNumber);
    }

    [Fact]
    public async Task Handle_NonExistentDormitory_ThrowsNotFoundException()
    {
        // Arrange
        var dormitoryId = Guid.NewGuid();
        
        var command = new CreateRoomCommand(
            dormitoryId,
            "406",
            Gender.Male,
            2,
            4);
        
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