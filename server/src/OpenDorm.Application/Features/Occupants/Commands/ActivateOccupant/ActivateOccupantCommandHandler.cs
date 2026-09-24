using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Occupants.Commands.ActivateOccupant;

public class ActivateOccupantCommandHandler(
    IUnitOfWork uow,
    IOccupantRepository repository) : IRequestHandler<ActivateOccupantCommand>
{
    public async Task Handle(ActivateOccupantCommand command, CancellationToken cancellationToken)
    {
        var occupant = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (occupant == null) throw new NotFoundException(nameof(Occupant), command.Id);
        
        occupant.Activate();
        await uow.SaveChangesAsync(cancellationToken);
    }
}