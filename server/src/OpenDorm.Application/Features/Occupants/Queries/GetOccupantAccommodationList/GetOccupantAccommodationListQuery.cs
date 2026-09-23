using MediatR;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;

public record GetOccupantAccommodationListQuery(Guid OccupantId) : IRequest<IReadOnlyCollection<AccommodationDto>>;