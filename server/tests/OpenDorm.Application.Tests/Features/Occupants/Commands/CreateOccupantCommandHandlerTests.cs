using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Enums;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands;

public class CreateOccupantCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private IOccupantRepository _repository = null!;
    private CreateOccupantCommandHandler _handler = null!;

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
        _handler = new CreateOccupantCommandHandler(_repository, unitOfWork);
    }

    [Theory]
    [InlineData("Иванович")]
    [InlineData(null)]
    public async Task Handle_ValidCommand_StoreDataToDatabase(string? patronymic)
    {
        // Assert
        const string lastName = "Иванов";
        const string firstName = "Иван";
        const Gender gender = Gender.Male;
        var birthDate = new DateOnly(2003, 01, 21);

        var command = new CreateOccupantCommand(lastName, firstName, patronymic, gender, birthDate);
        
        // Act
        var occupantId = await _handler.Handle(command, CancellationToken.None);
        _context.ChangeTracker.Clear();
        
        // Assert
        var occupant = await _repository.GetByIdAsync(occupantId);

        Assert.NotNull(occupant);
        Assert.Equal(lastName, occupant.LastName.Value);
        Assert.Equal(firstName, occupant.FirstName.Value);
        
        if (patronymic == null) Assert.Null(occupant.Patronymic);
        else Assert.Equal(patronymic, occupant.Patronymic!.Value);
        
        Assert.Equal(gender, occupant.Gender);
        Assert.Equal(birthDate, occupant.BirthDate.Value);
        Assert.True(occupant.IsActive);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}