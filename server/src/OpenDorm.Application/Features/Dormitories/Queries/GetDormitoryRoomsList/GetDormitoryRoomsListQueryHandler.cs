using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Common;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryRoomsList;

public class GetDormitoryRoomsListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetDormitoryRoomsListQuery, PagedResult<RoomListDto>>
{
    public async Task<PagedResult<RoomListDto>> Handle(
        GetDormitoryRoomsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = from room in dbContext.Rooms
            where EF.Property<Guid>(room, "DormitoryId") == request.DormitoryId
            where !request.IsActive.HasValue || room.IsActive == request.IsActive.Value
            where string.IsNullOrEmpty(request.Name) || room.Name.Value.Contains(request.Name)
            select room;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await (
                from room in query
                let occupantsCount = dbContext.Accommodations
                    .Count(a => a.RoomId == room.Id && a.CheckOutDate == null)
                orderby room.Name.Value
                select new RoomListDto(
                    room.Id,
                    room.Name.Value,
                    room.Capacity,
                    room.Gender,
                    room.IsActive,
                    room.FloorNumber,
                    occupantsCount))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomListDto>(items, totalCount, request.Page, request.PageSize);
    }
}