using OpenDorm.Application.Features.Occupants.Queries.Common;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;

public record AccommodationDto(
    Guid Id,
    DateTime CheckInDate,
    DateTime? CheckOutDate,
    AccommodationAddressDto Address);