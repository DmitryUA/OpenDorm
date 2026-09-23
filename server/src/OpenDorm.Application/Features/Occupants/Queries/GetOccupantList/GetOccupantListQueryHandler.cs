using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Occupants.Queries.Common;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;

public class GetOccupantListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOccupantListQuery, PagedResult<OccupantListDto>>
{
    public async Task<PagedResult<OccupantListDto>> Handle(GetOccupantListQuery request, CancellationToken cancellationToken)
    {
        var query = from occupant in dbContext.Occupants.AsNoTracking()
            where !request.Gender.HasValue || occupant.Gender == request.Gender
            where !request.IsActive.HasValue || occupant.IsActive == request.IsActive
            select occupant;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await (
                from occupant in query

                let currentAccommodation = (
                        from acc in dbContext.Accommodations
                        where EF.Property<Guid>(acc, "OccupantId") == occupant.Id
                              && acc.CheckOutDate == null

                        let room = dbContext.Rooms.FirstOrDefault(r => r.Id == acc.RoomId)
                        let dormitory = dbContext.Dormitories
                            .FirstOrDefault(d => d.Id == EF.Property<Guid>(room, "DormitoryId"))

                        select room != null && dormitory != null
                            ? new AccommodationAddressDto(
                                dormitory.Address.ToString(),
                                room.Name.Value)
                            : null)
                    .FirstOrDefault()

                let patronymicVo = occupant.Patronymic
                let patronymic = patronymicVo != null ? patronymicVo.Value : null

                orderby occupant.Id
                select new OccupantListDto(
                    occupant.Id,
                    occupant.LastName.Value,
                    occupant.FirstName.Value,
                    patronymic,
                    occupant.Gender,
                    occupant.BirthDate.Value,
                    occupant.IsActive,
                    currentAccommodation))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OccupantListDto>(items, totalCount, request.Page, request.PageSize);
    }
}