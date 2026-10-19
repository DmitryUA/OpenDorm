using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Integration;

public class UpdateOccupantCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();

    private OpenDormDbContext _context = null!;
    private OccupantRepository _repository = null!;
    private UpdateOccupantCommandHandler _handler = null!;

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
        _repository = new OccupantRepository(_context);
        _handler = new UpdateOccupantCommandHandler(_repository, uow);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #region Occupant Not Found
    [Fact]
    public async Task Handle_NonExistingOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateOccupantCommand(
            Guid.NewGuid(),
            NewLastName: "Петров",
            NewFirstName: "Пётр",
            NewPatronymic: null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Domain.Aggregates.Occupant.Occupant), exception.EntityName);
    }
    #endregion

    #region Personal Data Update
    [Fact]
    public async Task Handle_UpdatesPersonalData_PersistsToDatabase()
    {
        // Arrange
        var occupant = OccupantFactory.Create();
        await _repository.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateOccupantCommand(
            occupant.Id,
            NewLastName: "Петров",
            NewFirstName: "Пётр",
            NewPatronymic: "Петрович");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(occupant.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName.Value);
        Assert.Equal("Пётр", loaded.FirstName.Value);
        Assert.Equal("Петрович", loaded.Patronymic!.Value);
    }

    [Fact]
    public async Task Handle_UpdatesPersonalDataWithoutPatronymic_SetsPatronymicToNull()
    {
        // Arrange
        var occupant = OccupantFactory.Create(patronymic: "Иванович");
        await _repository.AddAsync(occupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpdateOccupantCommand(
            occupant.Id,
            NewLastName: "Петров",
            NewFirstName: "Пётр",
            NewPatronymic: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Assert
        var loaded = await _repository.GetByIdAsync(occupant.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName.Value);
        Assert.Equal("Пётр", loaded.FirstName.Value);
        Assert.Null(loaded.Patronymic);
    }
    #endregion
}