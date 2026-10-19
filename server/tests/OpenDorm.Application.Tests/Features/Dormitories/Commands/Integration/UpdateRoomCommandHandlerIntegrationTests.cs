using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Commands.UpdateRoom;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands.Integration;

public class UpdateRoomCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private DormitoryRepository _repository = null!;
    private OccupantRepository _occupantRepository = null!;
    private UpdateRoomCommandHandler _handler = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();
        var options = new DbContextOptionsBuilder<OpenDormDbContext>()
            .UseNpgsql(connectionString)
            .UseOccupantSearchIndexForTests()
            .Options;

        var encryptionService = new AesGcmEncryptionService(new byte[32]);
        _context = new OpenDormDbContext(options, encryptionService);
        await _context.Database.MigrateAsync();

        var uow = new UnitOfWork(_context);
        _repository = new DormitoryRepository(_context);
        _occupantRepository = new OccupantRepository(_context);
        _handler = new UpdateRoomCommandHandler(_context, _repository, uow);
    }

    #region Dormitory Not Found
    [Fact]
    public async Task Handle_NonExistingDormitory_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateRoomCommand(
            RoomId: Guid.NewGuid(),
            DormitoryId: Guid.NewGuid(),
            Name: "101");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Dormitory), exception.EntityName);
    }
    #endregion

    #region Gender Conflict
    [Fact]
    public async Task Handle_ChangeGenderWithActiveOccupantOfDifferentGender_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        await _repository.AddAsync(dormitory);
        
        var occupant = OccupantFactory.CreateCheckedIn(roomId, gender: Gender.Female);
        await _occupantRepository.AddAsync(occupant.Occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Gender: Gender.Male);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Cannot change room gender", exception.Message);
        Assert.Contains("active occupant has gender", exception.Message);
    }

    [Fact]
    public async Task Handle_ChangeGenderWithActiveOccupantOfSameGender_UpdatesSuccessfully()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        await _repository.AddAsync(dormitory);
        
        var occupant = OccupantFactory.CreateCheckedIn(roomId, gender: Gender.Male);
        await _occupantRepository.AddAsync(occupant.Occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Gender: Gender.Male);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(dormitory.Id);
        Assert.NotNull(loaded);
        var room = loaded.Rooms.First(r => r.Id == roomId);
        Assert.Equal(Gender.Male, room.Gender);
    }
    #endregion

    #region Capacity Conflict
    [Fact]
    public async Task Handle_DecreaseCapacityBelowActiveOccupantsCount_ThrowsDomainException()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom(capacity: new RoomCapacity(3));
        await _repository.AddAsync(dormitory);
        
        var occupant1 = OccupantFactory.CreateCheckedIn(roomId);
        var occupant2 = OccupantFactory.CreateCheckedIn(roomId);
        await _occupantRepository.AddAsync(occupant1.Occupant);
        await _occupantRepository.AddAsync(occupant2.Occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Capacity: 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Cannot change room capacity", exception.Message);
        Assert.Contains("there are 2 occupants living in the room", exception.Message);
    }

    [Fact]
    public async Task Handle_DecreaseCapacityAboveActiveOccupantsCount_UpdatesSuccessfully()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom(capacity: new RoomCapacity(5));
        await _repository.AddAsync(dormitory);
        
        var occupant = OccupantFactory.CreateCheckedIn(roomId);
        await _occupantRepository.AddAsync(occupant.Occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Capacity: 3);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(dormitory.Id);
        Assert.NotNull(loaded);
        var room = loaded.Rooms.First(r => r.Id == roomId);
        Assert.Equal(3, room.Capacity.Value);
    }
    #endregion

    #region Successful Updates
    [Fact]
    public async Task Handle_UpdateName_PersistsToDatabase()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Name: "205");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(dormitory.Id);
        Assert.NotNull(loaded);
        var room = loaded.Rooms.First(r => r.Id == roomId);
        Assert.Equal("205", room.Name.Value);
    }

    [Fact]
    public async Task Handle_UpdateAllFields_PersistsAllToDatabase()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateRoomCommand(
            RoomId: roomId,
            DormitoryId: dormitory.Id,
            Name: "301",
            Capacity: 4,
            Gender: Gender.Female);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(dormitory.Id);
        Assert.NotNull(loaded);
        var room = loaded.Rooms.First(r => r.Id == roomId);
        Assert.Equal("301", room.Name.Value);
        Assert.Equal(4, room.Capacity.Value);
        Assert.Equal(Gender.Female, room.Gender);
    }
    #endregion
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}