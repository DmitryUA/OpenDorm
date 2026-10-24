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
        var newLastName = new LastName(command.LastName);
        var newFirstName = new FirstName(command.FirstName);
        var newPatronymic = command.Patronymic != null
            ? new Patronymic(command.Patronymic)
            : null;
        
        var occupant = await repository.GetByIdAsync(command.OccupantId, cancellationToken) ??
                       throw new NotFoundException(nameof(Occupant), command.OccupantId);
        
        occupant.ChangeLastName(newLastName);
        occupant.ChangeFirstName(newFirstName);
        occupant.ChangePatronymic(newPatronymic);

        if (command.Phone != null)
        {
            var newPhone = new PhoneNumber(command.Phone);
            occupant.ChangePhone(newPhone);
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}