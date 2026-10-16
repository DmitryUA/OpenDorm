using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;

public record UpdateOccupantCommand(
    Guid OccupantId,
    string NewLastName,
    string NewFirstName,
    string? NewPatronymic = null) : IRequest;