using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;

public class UpdateOccupantCommandHandler(
    IOccupantRepository repository,
    IUnitOfWork uow) : IRequestHandler<UpdateOccupantCommand>
{
    public async Task Handle(UpdateOccupantCommand command, CancellationToken cancellationToken)
    {
        var newLastName = new LastName(command.NewLastName);
        var newFirstName = new FirstName(command.NewFirstName);
        var newPatronymic = command.NewPatronymic != null
            ? new Patronymic(command.NewPatronymic)
            : null;
        
        var occupant = await repository.GetByIdAsync(command.OccupantId, cancellationToken) ??
                       throw new NotFoundException(nameof(Occupant), command.OccupantId);
        
        occupant.ChangeLastName(newLastName);
        occupant.ChangeFirstName(newFirstName);
        occupant.ChangePatronymic(newPatronymic);

        await uow.SaveChangesAsync(cancellationToken);
    }
}