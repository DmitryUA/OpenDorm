using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;

public class CreateOccupantCommandHandler(
    IOccupantRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOccupantCommand, Guid>
{
    public async Task<Guid> Handle(CreateOccupantCommand command, CancellationToken cancellationToken)
    {
        var lastName = new LastName(command.LastName);
        var firstName = new FirstName(command.FirstName);
        var patronymic = command.Patronymic == null ? null : new Patronymic(command.Patronymic);
        var birthDate = new BirthDate(command.BirthDate);

        var occupant = new Occupant(Guid.NewGuid(), lastName, firstName, patronymic, command.Gender, birthDate);

        await repository.AddAsync(occupant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return occupant.Id;
    }
}