using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Occupants.Commands.Unit;

public class CheckOutOccupantCommandHandlerUnitTests
{
    [Fact]
    public async Task Handle_NonExistOccupant_ThrowsNotFoundException()
    {
        // Arrange
        var uow = Substitute.For<IUnitOfWork>();
        var repository = Substitute.For<IOccupantRepository>();
        var nonExistId = Guid.NewGuid();
        
        // Настраиваем NSubstitute: при вызове GetByIdAsync вернуть null
        repository.GetByIdAsync(nonExistId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Occupant?>(null));

        var handler = new CheckOutOccupantCommandHandler(uow, repository);
        var command = new CheckOutOccupantCommand(nonExistId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
            
        Assert.Equal(nameof(Occupant), exception.EntityName);
        Assert.Equal(nonExistId, exception.EntityId);
    }
}