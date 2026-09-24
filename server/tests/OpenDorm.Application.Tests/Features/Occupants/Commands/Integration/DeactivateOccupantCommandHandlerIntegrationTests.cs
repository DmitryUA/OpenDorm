using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Integration;

public class DeactivateOccupantCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private IOccupantRepository _repository = null!;
    private DeactivateOccupantCommandHandler _handler = null!;
    
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
        _handler = new DeactivateOccupantCommandHandler(unitOfWork, _repository);
    }

    [Fact]
    public async Task Handle_ActiveOccupant_MakeOccupantInactive()
    {
        // Arrange
        var occupant = OccupantFactory.Create();

        await _repository.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new DeactivateOccupantCommand(occupant.Id);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        var occupantStatus = await _context.Occupants
            .Where(o => o.Id == occupant.Id)
            .Select(o => o.IsActive)
            .FirstOrDefaultAsync();
        
        Assert.False(occupantStatus);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}