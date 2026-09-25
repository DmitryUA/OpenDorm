using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;

public record RoomOccupancyHistoryListDto(
    Guid OccupantId,
    Guid AccommodationId,
    string LastName,
    string FirstName,
    string? Patronymic,
    Gender Gender,
    DateOnly BirthDate,
    DateTime CheckInDate,
    DateTime? CheckOutDate);