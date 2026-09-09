using OpenDorm.Domain.Enums;

namespace OpenDorm.API.Contracts;

public record CreateRoomRequest(
    string RoomName,
    Gender Gender,
    int Capacity,
    int FloorNumber);