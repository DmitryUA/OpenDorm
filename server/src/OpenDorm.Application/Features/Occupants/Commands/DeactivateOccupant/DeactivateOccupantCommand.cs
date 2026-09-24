using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;

public record DeactivateOccupantCommand(Guid Id) : IRequest;