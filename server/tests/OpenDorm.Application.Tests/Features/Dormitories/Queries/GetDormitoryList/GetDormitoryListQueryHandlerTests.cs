using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;
using OpenDorm.Application.Tests.Helpers;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.GetDormitoryList;

public class GetDormitoryListQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetDormitoryListQueryHandler _handler;

    public GetDormitoryListQueryHandlerTests()
    {
        _dbContext = Substitute.For<IApplicationDbContext>();
        _handler = new GetDormitoryListQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithDormitories_ReturnsSortedDtos()
    {
        // Arrange
        var dormitories = new List<Dormitory>
        {
            CreateDormitory("Москва", "Ленина", "10", 5),
            CreateDormitory("Казань", "Баумана", "5", 3),
            CreateDormitory("Москва", "Арбат", "20", 4),
        };

        // Используем наш TestAsyncEnumerable
        _dbContext.Dormitories.Returns(new TestAsyncEnumerable<Dormitory>(dormitories));

        // Act
        var result = await _handler.Handle(
            new GetDormitoryListQuery(), 
            CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    
        var resultList = result.ToList();
        Assert.Equal("Казань", resultList[0].City);
        Assert.Equal("Москва", resultList[1].City);
        Assert.Equal("Москва", resultList[2].City);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        _dbContext.Dormitories.Returns(new TestAsyncEnumerable<Dormitory>([]));

        // Act
        var result = await _handler.Handle(
            new GetDormitoryListQuery(), 
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    private static Dormitory CreateDormitory(string city, string street, string house, int floors)
    {
        var cityVo = new City(city);
        var streetVo = new Street(street);
        var houseVo = new HouseNumber(house);
        var address = new Address(cityVo, streetVo, houseVo);
        
        return new Dormitory(
            Guid.NewGuid(),
            address,
            floors);
    }
}