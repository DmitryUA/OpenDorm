using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateRoom;

public class UpdateRoomCommandHandler(
    IApplicationDbContext context,
    IDormitoryRepository repository,
    IUnitOfWork uow) : IRequestHandler<UpdateRoomCommand>
{
    public async Task Handle(UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        var dormitory = await repository.GetByIdAsync(command.DormitoryId, cancellationToken) ??
                        throw new NotFoundException(nameof(Dormitory), command.DormitoryId);
        
        if (command.Gender.HasValue)
        {
            var conflict = await context.Accommodations
                .Where(a => a.RoomId == command.RoomId && a.CheckOutDate == null)
                .Join(
                    context.Occupants,
                    a => EF.Property<Guid>(a, "OccupantId"),
                    o => o.Id,
                    (a, o) => new { AccommodationId = a.Id, o.Gender })
                .FirstOrDefaultAsync(cancellationToken);

            if (conflict is not null && conflict.Gender != command.Gender.Value)
            {
                throw new DomainException(
                    $"Cannot change room gender to {command.Gender.Value}: " +
                    $"active occupant has gender {conflict.Gender}. " +
                    $"Accommodation id: '{conflict.AccommodationId}'.");
            }
        }

        if (command.Capacity.HasValue)
        {
            var activeAccommodationsCount = await context.Accommodations
                .CountAsync(a => a.RoomId == command.RoomId && a.CheckOutDate == null, cancellationToken);

            if (activeAccommodationsCount > command.Capacity.Value)
                throw new DomainException($"Cannot change room capacity to {command.Capacity.Value}: " +
                                          $"there are {activeAccommodationsCount} occupants living in the room.");
        }
        
        dormitory.UpdateRoom(command.RoomId, room =>
        {
            if (command.Name != null) room.ChangeName(new RoomName(command.Name));
            if (command.Gender.HasValue) room.ChangeGender(command.Gender.Value);
            if (command.Capacity.HasValue) room.ChangeCapacity(new RoomCapacity(command.Capacity.Value));
        });

        await uow.SaveChangesAsync(cancellationToken);
    }
}