using MockQueryable.NSubstitute;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Dormitories.Queries.Unit;

public class GetRoomOccupantsQueryHandlerUnitTests
{
    [Fact]
    public async Task Handle_NonExistRoom_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistRoomId = Guid.NewGuid();
        var query = new GetRoomOccupantsQuery(nonExistRoomId);
    
        var context = Substitute.For<IApplicationDbContext>();
        var emptyRoomsList = new List<Room>(); 
        var mockRoomsDbSet = emptyRoomsList.BuildMockDbSet();
        
        context.Rooms.Returns(mockRoomsDbSet);

        var handler = new GetRoomOccupantsQueryHandler(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None)
        );
    
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(nonExistRoomId, exception.EntityId);
    }
}