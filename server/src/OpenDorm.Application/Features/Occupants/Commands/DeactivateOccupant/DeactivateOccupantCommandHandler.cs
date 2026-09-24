using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;

public class DeactivateOccupantCommandHandler(
    IUnitOfWork uow,
    IOccupantRepository repository)
    : IRequestHandler<DeactivateOccupantCommand>
{
    public async Task Handle(DeactivateOccupantCommand command, CancellationToken cancellationToken)
    {
        var occupant = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (occupant == null) throw new NotFoundException(nameof(Occupant), command.Id);
        
        occupant.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);
    }
}