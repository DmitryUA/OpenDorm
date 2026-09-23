using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Occupants.Queries.Common;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;

public class GetOccupantAccommodationListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOccupantAccommodationListQuery, IReadOnlyCollection<AccommodationDto>>
{
    public async Task<IReadOnlyCollection<AccommodationDto>> Handle(
        GetOccupantAccommodationListQuery request,
        CancellationToken cancellationToken)
    {
        var occupantExists = await dbContext.Occupants
            .AsNoTracking()
            .AnyAsync(o => o.Id == request.OccupantId, cancellationToken);
    
        if (!occupantExists)
            throw new NotFoundException(nameof(Occupant), request.OccupantId);
        
        var accommodationsDto = await (
            from accommodation in dbContext.Accommodations.AsNoTracking()
            where EF.Property<Guid>(accommodation, "OccupantId") == request.OccupantId

            let room = dbContext.Rooms
                .FirstOrDefault(r => r.Id == accommodation.RoomId)

            let dormitory = dbContext.Dormitories
                .FirstOrDefault(d => d.Id == EF.Property<Guid>(room, "DormitoryId"))

            let accommodationAddressDto = new AccommodationAddressDto(dormitory.Address.ToString(), room.Name.Value)

            select new AccommodationDto(
                accommodation.Id,
                accommodation.CheckInDate,
                accommodation.CheckOutDate,
                accommodationAddressDto)
        ).ToListAsync(cancellationToken);

        return accommodationsDto;
    }
}