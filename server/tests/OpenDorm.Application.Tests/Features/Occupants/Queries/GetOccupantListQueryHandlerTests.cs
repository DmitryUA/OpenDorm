using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;
using OpenDorm.Domain.Enums;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Services;
using OpenDorm.Shared.Tests.Factories;
using Testcontainers.PostgreSql;

namespace OpenDorm.Application.Tests.Features.Occupants.Queries;

public class GetOccupantListQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("opendorm_test")
        .Build();
    
    private OpenDormDbContext _context = null!;
    private GetOccupantListQueryHandler _handler = null!;
    
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

        _handler = new GetOccupantListQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_NoFilters_WillReturnListOfTwoItems()
    {
        // Arrange
        var (dormitory, roomId) = DormitoryFactory.CreateWithOneRoom();
        var activeOccupant = OccupantFactory.CreateCheckedIn(roomId).Occupant;
        var inactiveOccupant = OccupantFactory.Create(gender: Gender.Female);
        
        inactiveOccupant.Deactivate();

        await _context.AddAsync(dormitory);
        await _context.AddAsync(activeOccupant);
        await _context.AddAsync(inactiveOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetOccupantListQuery(Page: 1, PageSize: 2);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        
        var loadedActiveOccupant = result.Items.FirstOrDefault(o => o.IsActive);
        
        Assert.NotNull(loadedActiveOccupant);
        Assert.Equal(activeOccupant.Id, loadedActiveOccupant.Id);
        Assert.Equal(activeOccupant.Gender, loadedActiveOccupant.Gender);
        Assert.Equal(activeOccupant.LastName.Value, loadedActiveOccupant.LastName);
        Assert.Equal(activeOccupant.FirstName.Value, loadedActiveOccupant.FirstName);
        Assert.Equal(activeOccupant.Patronymic?.Value, loadedActiveOccupant.Patronymic);
        Assert.Equal(activeOccupant.BirthDate.Value, loadedActiveOccupant.BirthDate);
        Assert.NotNull(loadedActiveOccupant.CurrentAccommodation);
        Assert.Equal(dormitory.Address.ToString(), loadedActiveOccupant.CurrentAccommodation.DormitoryAddress);
        Assert.Equal(dormitory.Rooms.First().Name.Value, loadedActiveOccupant.CurrentAccommodation.RoomName);

        var loadedInactiveOccupant = result.Items.FirstOrDefault(o => !o.IsActive);

        Assert.NotNull(loadedInactiveOccupant);
        Assert.Null(loadedInactiveOccupant.CurrentAccommodation);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Handle_FilterByGender_WillReturnsListOfOneItem(Gender gender)
    {
        // Arrange
        var maleOccupant = OccupantFactory.Create(gender: Gender.Male);
        var femaleOccupant = OccupantFactory.Create(gender: Gender.Female);

        await _context.AddAsync(maleOccupant);
        await _context.AddAsync(femaleOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetOccupantListQuery(gender);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Single(result.Items);
        Assert.Equal(gender, result.Items.First().Gender);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_FilterByStatus_WillReturnsListOfOneItem(bool status)
    {
        // Arrange
        var activeOccupant = OccupantFactory.Create();
        var inactiveOccupant = OccupantFactory.Create();
        
        inactiveOccupant.Deactivate();

        await _context.AddAsync(activeOccupant);
        await _context.AddAsync(inactiveOccupant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var query = new GetOccupantListQuery(IsActive: status);
        
        // Assert
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(status, result.Items.First().IsActive);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}