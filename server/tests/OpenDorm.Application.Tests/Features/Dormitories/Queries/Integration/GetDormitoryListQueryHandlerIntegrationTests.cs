using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.Integration;

public class GetDormitoryListQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetDormitoryListQueryHandler _handler = null!;
    
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

        _handler = new GetDormitoryListQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithDormitories_ReturnsSortedDtos()
    {
        // Arrange
        var dormitories = new List<Dormitory>
        {
            DormitoryFactory.Create(AddressFactory.Moscow(), 5),
            DormitoryFactory.Create(AddressFactory.Kazan(), 3),
            DormitoryFactory.Create(AddressFactory.Create(street: "Арбат"))
        };

        await _context.DormitoriesDbSet.AddRangeAsync(dormitories);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _handler.Handle(
            new GetDormitoryListQuery(), 
            CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        
        var resultList = result.ToList();
        Assert.Equal("Казань", resultList[0].City);
        Assert.Equal("Москва", resultList[1].City);
        Assert.Equal("Арбат", resultList[1].Street);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Act
        var result = await _handler.Handle(
            new GetDormitoryListQuery(), 
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}