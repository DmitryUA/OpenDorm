using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;

public class GetRoomOccupantsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomOccupantsQuery, IReadOnlyCollection<RoomOccupantDto>>
{
    public async Task<IReadOnlyCollection<RoomOccupantDto>> Handle(
        GetRoomOccupantsQuery request,
        CancellationToken cancellationToken)
    {
        var roomIsExist = await dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
        if (!roomIsExist) throw new NotFoundException(nameof(Room), request.RoomId);
        
        var occupantDtos = await (
            from occupant in dbContext.Occupants.AsNoTracking()
            join accommodation in dbContext.Accommodations
                on occupant.Id equals EF.Property<Guid>(accommodation, "OccupantId") 
            where accommodation.RoomId == request.RoomId
                  && accommodation.CheckOutDate == null
            let patronymic = occupant.Patronymic != null ? occupant.Patronymic.Value : null
            select new RoomOccupantDto(
                occupant.Id,
                occupant.LastName.Value,
                occupant.FirstName.Value,
                patronymic,
                occupant.BirthDate.Value)
        ).ToListAsync(cancellationToken);

        return occupantDtos;
    }
}