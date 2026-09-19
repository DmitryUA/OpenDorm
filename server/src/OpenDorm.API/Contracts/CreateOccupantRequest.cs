using OpenDorm.Domain.Enums;

namespace OpenDorm.API.Contracts;

public record CreateOccupantRequest(
    string LastName,
    string FirstName,
    string? Patronymic,
    Gender Gender,
    DateOnly BirthDate);