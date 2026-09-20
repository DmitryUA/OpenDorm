using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;

public record CheckOutOccupantCommand(Guid OccupantId) : IRequest;