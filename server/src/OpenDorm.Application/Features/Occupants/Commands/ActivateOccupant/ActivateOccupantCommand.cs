using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.ActivateOccupant;

public record ActivateOccupantCommand(Guid Id) : IRequest;