using OpenDorm.Application.Features.Occupants.Queries.Common;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;

public record OccupantListDto(
    Guid Id,
    string LastName,
    string FirstName,
    string? Patronymic,
    Gender Gender,
    DateOnly BirthDate,
    bool IsActive,
    AccommodationAddressDto? CurrentAccommodation);