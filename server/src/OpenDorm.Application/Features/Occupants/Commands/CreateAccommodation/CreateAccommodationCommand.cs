using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;

public record CreateAccommodationCommand(
    Guid RoomId,
    Guid OccupantId) : IRequest<Guid>;