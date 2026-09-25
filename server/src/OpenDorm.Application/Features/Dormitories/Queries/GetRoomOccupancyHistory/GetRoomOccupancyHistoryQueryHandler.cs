using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Common;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;

public class GetRoomOccupancyHistoryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomOccupancyHistoryQuery, PagedResult<RoomOccupancyHistoryListDto>>
{
    public async Task<PagedResult<RoomOccupancyHistoryListDto>> Handle(
        GetRoomOccupancyHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var isRoomExist = await dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);

        if (!isRoomExist) throw new NotFoundException(nameof(Room), request.RoomId);
        
        var startCheckInDate = request.StartCheckInDate?.ToDateTime(TimeOnly.MinValue);
        var endCheckInDate = request.EndCheckInDate?.ToDateTime(TimeOnly.MaxValue);

        var query = dbContext.Accommodations
            .AsNoTracking()
            .Join(
                dbContext.Occupants.AsNoTracking(),
                accommodation => EF.Property<Guid>(accommodation, "OccupantId"),
                occupant => occupant.Id,
                (accommodation, occupant) => new { accommodation, occupant })
            .Where(x => x.accommodation.RoomId == request.RoomId);

        if (startCheckInDate.HasValue)
            query = query.Where(x => x.accommodation.CheckInDate >= startCheckInDate.Value);

        if (endCheckInDate.HasValue)
            query = query.Where(x => x.accommodation.CheckInDate <= endCheckInDate.Value);

        query = request.SortOrder switch
        {
            SortOrder.Ascending => query.OrderBy(x => x.accommodation.CheckInDate),
            SortOrder.Descending => query.OrderByDescending(x => x.accommodation.CheckInDate),
            _ => throw new ArgumentOutOfRangeException(nameof(request.SortOrder))
        };
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Select(x => new RoomOccupancyHistoryListDto(
                x.occupant.Id,
                x.accommodation.Id,
                x.occupant.LastName.Value,
                x.occupant.FirstName.Value,
                x.occupant.Patronymic != null ? x.occupant.Patronymic.Value : null,
                x.occupant.Gender,
                x.occupant.BirthDate.Value,
                x.accommodation.CheckInDate,
                x.accommodation.CheckOutDate)
            ).Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomOccupancyHistoryListDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }
}