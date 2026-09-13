using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Dormitories.Commands.DeactivateRoom;

public class DeactivateRoomCommandHandler(
    IDormitoryRepository repository,
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateRoomCommand>
{
    public async Task Handle(DeactivateRoomCommand command, CancellationToken cancellationToken)
    {
        var dormitory = await repository.GetByIdAsync(command.DormitoryId, cancellationToken);

        if (dormitory == null) throw new NotFoundException(nameof(Dormitory), command.DormitoryId);

        var activeAccommodationId = await dbContext.Accommodations
            .AsNoTracking()
            .Where(a => a.RoomId == command.RoomId && a.CheckOutDate == null)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeAccommodationId != null)
            throw new DomainException(
                $"It is impossible to deactivate a room that is currently occupied. Accommodation id: '{activeAccommodationId}'");
        
        dormitory.DeactivateRoom(command.RoomId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}