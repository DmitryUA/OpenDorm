using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;

public class CreateAccommodationCommandHandler(
    IApplicationDbContext dbContext, 
    IOccupantRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAccommodationCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccommodationCommand command, CancellationToken cancellationToken)
    {
        var roomInfo = await dbContext.Rooms
            .Where(r => r.Id == command.RoomId)
            .Select(r => new
            {
                r.IsActive,
                HasFreePlaces = dbContext.Accommodations
                    .Count(a => a.RoomId == r.Id && a.CheckOutDate == null) < r.Capacity
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (roomInfo == null) throw new NotFoundException(nameof(Room), command.RoomId);
        if (!roomInfo.IsActive) throw new DomainException($"Room is inactive. Room id: '{command.RoomId}'.");
        if (!roomInfo.HasFreePlaces)
            throw new DomainException($"There are no empty seats in the room. Room id: '{command.RoomId}'.");

        var occupant = await repository.GetByIdAsync(command.OccupantId, cancellationToken);

        if (occupant == null) throw new NotFoundException(nameof(Occupant), command.OccupantId);

        var accommodationId = occupant.CheckIn(command.RoomId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return accommodationId;
    }
}