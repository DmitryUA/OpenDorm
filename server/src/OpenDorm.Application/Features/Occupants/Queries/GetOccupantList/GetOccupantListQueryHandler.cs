using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Abstractions.Search;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Occupants.Queries.Common;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;

public class GetOccupantListQueryHandler(
    IApplicationDbContext dbContext,
    IOccupantSearchIndex searchIndex)
    : IRequestHandler<GetOccupantListQuery, PagedResult<OccupantListDto>>
{
    public async Task<PagedResult<OccupantListDto>> Handle(GetOccupantListQuery request, CancellationToken cancellationToken)
    {
        var lastNameHash = searchIndex.ComputeHash(request.LastName);
        var firstNameHash = searchIndex.ComputeHash(request.FirstName);
        var patronymicHash = searchIndex.ComputeHash(request.Patronymic);

        var query = dbContext.Occupants.AsNoTracking()
            .Where(occupant => !request.Gender.HasValue || occupant.Gender == request.Gender)
            .Where(occupant => !request.IsActive.HasValue || occupant.IsActive == request.IsActive);

        if (lastNameHash is not null)
            query = query.Where(occupant =>
                EF.Property<byte[]?>(occupant, "last_name_search_hash") == lastNameHash);

        if (firstNameHash is not null)
            query = query.Where(occupant =>
                EF.Property<byte[]?>(occupant, "first_name_search_hash") == firstNameHash);

        if (patronymicHash is not null)
            query = query.Where(occupant =>
                EF.Property<byte[]?>(occupant, "patronymic_search_hash") == patronymicHash);

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
                    currentAccommodation,
                    occupant.Phone.Value))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OccupantListDto>(items, totalCount, request.Page, request.PageSize);
    }
}