using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;

public record GetRoomOccupantsQuery(Guid RoomId) : IRequest<IReadOnlyCollection<RoomOccupantDto>>;