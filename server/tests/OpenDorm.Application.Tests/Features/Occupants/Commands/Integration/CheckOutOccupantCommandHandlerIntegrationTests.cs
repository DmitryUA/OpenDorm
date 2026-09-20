using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Integration;

public class CheckOutOccupantCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private IUnitOfWork _unitOfWork = null!;
    private OpenDormDbContext _context = null!;
    private IOccupantRepository _repository = null!;
    private CheckOutOccupantCommandHandler _handlerIntegration = null!;

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
        _repository = new OccupantRepository(_context);
        _handlerIntegration = new CheckOutOccupantCommandHandler(_unitOfWork, _repository);
    }

    [Fact]
    public async Task Handle_OccupantWithActiveAccommodation_MakeAccommodationInactive()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        var (occupant, accommodationId) = OccupantFactory.CreateCheckedIn(roomId);

        await _context.DormitoriesDbSet.AddAsync(dormitory);
        await _context.OccupantsDbSet.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CheckOutOccupantCommand(occupant.Id);
        
        // Act
        await _handlerIntegration.Handle(command, CancellationToken.None);
        
        // Assert
        var checkOutDateIsNotNull = await _context.Accommodations
            .Where(a => a.Id == accommodationId)
            .Select(a=>a.CheckOutDate != null)
            .FirstAsync();
        
        Assert.True(checkOutDateIsNotNull);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}