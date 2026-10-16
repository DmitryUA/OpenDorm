using MediatR;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateRoomCommand;

public record UpdateRoomCommand(
    Guid RoomId,
    Guid DormitoryId,
    string? Name = null,
    int? Capacity = null,
    Gender? Gender = null) : IRequest;