using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;

public record UpdateOccupantCommand(
    Guid OccupantId,
    string LastName,
    string FirstName,
    string? Patronymic = null,
    string? Phone = null) : IRequest;