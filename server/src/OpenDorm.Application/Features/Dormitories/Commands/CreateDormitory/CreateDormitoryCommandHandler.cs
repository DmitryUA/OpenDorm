using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;

public class CreateDormitoryCommandHandler(IDormitoryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDormitoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateDormitoryCommand request, CancellationToken cancellationToken)
    {
        var city = new City(request.City);
        var street = new Street(request.Street);
        var house = new HouseNumber(request.House);
        var address = new Address(city, street, house);

        var dormitory = new Dormitory(Guid.NewGuid(), address, request.FloorCount);

        await repository.AddAsync(dormitory);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return dormitory.Id;
    }
}