namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;

public record RoomOccupantDto(
    Guid Id,
    string LastName,
    string FirstName,
    string? Patronymic,
    DateOnly BirthDate);