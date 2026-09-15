using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Dormitories.Commands.ActivateRoom;

public class ActivateRoomCommandHandler(
    IDormitoryRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateRoomCommand>
{
    public async Task Handle(ActivateRoomCommand command, CancellationToken cancellationToken)
    {
        var dormitory = await repository.GetByIdAsync(command.DormitoryId, cancellationToken);

        if (dormitory == null) throw new NotFoundException(nameof(Dormitory), command.DormitoryId);
        
        dormitory.ActivateRoom(command.RoomId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}