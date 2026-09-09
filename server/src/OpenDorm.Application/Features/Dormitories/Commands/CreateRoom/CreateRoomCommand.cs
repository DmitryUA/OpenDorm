using MediatR;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateRoom;

public record CreateRoomCommand(
    Guid DormitoryId,
    string RoomName,
    Gender Gender,
    int Capacity,
    int FloorNumber) : IRequest<Guid>;