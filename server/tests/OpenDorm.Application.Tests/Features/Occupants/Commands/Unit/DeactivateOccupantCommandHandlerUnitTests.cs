using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Unit;

public class DeactivateOccupantCommandHandlerUnitTests
{
    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistOccupantId = Guid.NewGuid();
        var command = new DeactivateOccupantCommand(nonExistOccupantId);
        var mockUow = Substitute.For<IUnitOfWork>();
        var mockRepository = Substitute.For<IOccupantRepository>();
        
        mockRepository.GetByIdAsync(nonExistOccupantId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Occupant?>(null));

        var handler = new DeactivateOccupantCommandHandler(mockUow, mockRepository);
        
        // Act & Asser
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistOccupantId, exception.EntityId);
    }
}