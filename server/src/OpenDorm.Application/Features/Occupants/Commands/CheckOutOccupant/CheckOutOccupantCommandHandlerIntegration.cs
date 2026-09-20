using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;

public class CheckOutOccupantCommandHandler(
    IUnitOfWork unitOfWork,
    IOccupantRepository repository) : IRequestHandler<CheckOutOccupantCommand>
{
    public async Task Handle(CheckOutOccupantCommand command, CancellationToken cancellationToken)
    {
        var occupant = await repository.GetByIdAsync(command.OccupantId, cancellationToken);

        if (occupant == null) throw new NotFoundException(nameof(Occupant), command.OccupantId);
        
        occupant.CheckOut();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}