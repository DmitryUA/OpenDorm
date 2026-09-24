using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.ActivateOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Unit;

public class ActivateOccupantCommandHandlerUnitTests
{
    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistOccupantId = Guid.NewGuid();
        var mockUow = Substitute.For<IUnitOfWork>();
        var mockRepository = Substitute.For<IOccupantRepository>();

        mockRepository.GetByIdAsync(nonExistOccupantId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Occupant?>(null));

        var handler = new ActivateOccupantCommandHandler(mockUow, mockRepository);
        var command = new ActivateOccupantCommand(nonExistOccupantId);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistOccupantId, exception.EntityId);
    }
}