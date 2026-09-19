using MediatR;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;

public record CreateOccupantCommand(
    string LastName,
    string FirstName,
    string? Patronymic,
    Gender Gender,
    DateOnly BirthDate) : IRequest<Guid>;