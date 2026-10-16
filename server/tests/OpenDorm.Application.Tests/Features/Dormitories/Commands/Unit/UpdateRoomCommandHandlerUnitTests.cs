using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Commands.UpdateRoomCommand;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands.Unit;

public class UpdateRoomCommandHandlerUnitTests
{
    private readonly IDormitoryRepository _mockRepository;
    private readonly UpdateRoomCommandHandler _handler;

    public UpdateRoomCommandHandlerUnitTests()
    {
        var mockContext = Substitute.For<IApplicationDbContext>();
        _mockRepository = Substitute.For<IDormitoryRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();
        _handler = new UpdateRoomCommandHandler(mockContext, _mockRepository, mockUow);
    }

    #region Dormitory Not Found
    [Fact]
    public async Task Handle_NonExistingDormitory_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingDormitoryId = Guid.NewGuid();
        var command = new UpdateRoomCommand(
            RoomId: Guid.NewGuid(),
            DormitoryId: nonExistingDormitoryId,
            Name: "101");

        _mockRepository
            .GetByIdAsync(nonExistingDormitoryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Dormitory?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Dormitory), exception.EntityName);
        Assert.Equal(nonExistingDormitoryId, exception.EntityId);
    }
    #endregion

    #region Room Not Found
    [Fact]
    public async Task Handle_NonExistingRoom_ThrowsNotFoundException()
    {
        // Arrange
        var dormitory = DormitoryFactory.Create();
        var nonExistingRoomId = Guid.NewGuid();
        var command = new UpdateRoomCommand(
            RoomId: nonExistingRoomId,
            DormitoryId: dormitory.Id,
            Name: "101");

        _mockRepository
            .GetByIdAsync(dormitory.Id, Arg.Any<CancellationToken>())
            .Returns(dormitory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Room), exception.EntityName);
        Assert.Equal(nonExistingRoomId, exception.EntityId);
    }
    #endregion
}