using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Commands.ActivateRoom;

public record ActivateRoomCommand(
    Guid DormitoryId,
    Guid RoomId) : IRequest;