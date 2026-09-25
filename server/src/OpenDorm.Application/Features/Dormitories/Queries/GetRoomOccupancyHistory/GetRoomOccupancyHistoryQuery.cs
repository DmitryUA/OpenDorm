using MediatR;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Common;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;

public record GetRoomOccupancyHistoryQuery(
    Guid RoomId,
    SortOrder SortOrder = SortOrder.Descending,
    DateOnly? StartCheckInDate = null,
    DateOnly? EndCheckInDate = null,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<RoomOccupancyHistoryListDto>>;