using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Commands.UpdateDormitory;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands.Integration;

public class UpdateDormitoryCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private DormitoryRepository _repository = null!;
    private UpdateDormitoryCommandHandler _handler = null!;

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

        var uow = new UnitOfWork(_context);
        _repository = new DormitoryRepository(_context);
        _handler = new UpdateDormitoryCommandHandler(_repository, uow);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAddressAndPersistsToDatabase()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        await _repository.AddAsync(dormitory);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateDormitoryCommand(
            dormitory.Id,
            City: "Санкт-Петербург",
            Street: "Невский проспект",
            House: "28");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(dormitory.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Санкт-Петербург", loaded.Address.City.Value);
        Assert.Equal("Невский проспект", loaded.Address.Street.Value);
        Assert.Equal("28", loaded.Address.House.Value);
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}