using MediatR;

namespace OpenDorm.Application.Features.Occupants.Commands.TransferOccupant;

public record TransferOccupantCommand(Guid OccupantId, Guid RoomId) : IRequest<Guid>;