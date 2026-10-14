using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateRoom;

public class CreateRoomCommandHandler(IDormitoryRepository repository, IUnitOfWork unitOfWork)
: IRequestHandler<CreateRoomCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var roomName = new RoomName(command.RoomName);
        var roomCapacity = new RoomCapacity(command.Capacity);
        var floorNumber = new FloorNumber(command.FloorNumber);
        
        var dormitory = await repository.GetByIdAsync(command.DormitoryId, cancellationToken);
        if (dormitory == null) throw new NotFoundException(nameof(Dormitory), command.DormitoryId);
        
        var roomId = dormitory.AddRoom(roomName, command.Gender, roomCapacity, floorNumber);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return roomId;
    }
}