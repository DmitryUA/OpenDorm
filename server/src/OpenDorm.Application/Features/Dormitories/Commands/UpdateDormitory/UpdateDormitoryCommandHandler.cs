using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateDormitory;

public class UpdateDormitoryCommandHandler(
    IDormitoryRepository repository,
    IUnitOfWork uow) : IRequestHandler<UpdateDormitoryCommand>
{
    public async Task Handle(UpdateDormitoryCommand command, CancellationToken cancellationToken)
    {
        var city = new City(command.City);
        var street = new Street(command.Street);
        var house = new HouseNumber(command.House);
        var address = new Address(city, street, house);

        var dormitory = await repository.GetByIdAsync(command.DormitoryId, cancellationToken) ??
                        throw new NotFoundException(nameof(Dormitory), command.DormitoryId);
        
        dormitory.ChangeAddress(address);

        await uow.SaveChangesAsync(cancellationToken);
    }
}