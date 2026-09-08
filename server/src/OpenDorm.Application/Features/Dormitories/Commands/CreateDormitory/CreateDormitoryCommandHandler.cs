using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;

public class CreateDormitoryCommandHandler(IDormitoryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDormitoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateDormitoryCommand command, CancellationToken cancellationToken)
    {
        var city = new City(command.City);
        var street = new Street(command.Street);
        var house = new HouseNumber(command.House);
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(Guid.NewGuid(), address, command.FloorCount);

        await repository.AddAsync(dormitory);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return dormitory.Id;
    }
}