using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryRoomsList;

public record RoomListDto(
    Guid Id,
    string Name,
    int Capacity,
    Gender Gender,
    bool IsActive,
    int FloorNumber,
    int OccupantCount);