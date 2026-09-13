using MediatR;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Common;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryRoomsList;

public record GetDormitoryRoomsListQuery(
    Guid DormitoryId,
    string? Name = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<RoomListDto>>;