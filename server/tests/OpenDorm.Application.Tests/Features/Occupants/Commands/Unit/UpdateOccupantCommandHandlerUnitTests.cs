using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Unit;

public class UpdateOccupantCommandHandlerUnitTests
{
    private readonly IOccupantRepository _mockRepository;
    private readonly UpdateOccupantCommandHandler _handler;

    public UpdateOccupantCommandHandlerUnitTests()
    {
        _mockRepository = Substitute.For<IOccupantRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();
        _handler = new UpdateOccupantCommandHandler(_mockRepository, mockUow);
    }

    [Fact]
    public async Task Handle_NonExistingOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingOccupantId = Guid.NewGuid();
        var command = new UpdateOccupantCommand(
            nonExistingOccupantId,
            NewLastName: "Петров",
            NewFirstName: "Пётр",
            NewPatronymic: null);

        _mockRepository
            .GetByIdAsync(nonExistingOccupantId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Occupant?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistingOccupantId, exception.EntityId);
    }
}