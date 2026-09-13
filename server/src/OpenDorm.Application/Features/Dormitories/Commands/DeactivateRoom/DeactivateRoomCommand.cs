using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Commands.DeactivateRoom;

public record DeactivateRoomCommand(
    Guid DormitoryId,
    Guid RoomId) : IRequest;