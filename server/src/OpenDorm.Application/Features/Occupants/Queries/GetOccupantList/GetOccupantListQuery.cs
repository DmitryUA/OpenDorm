using MediatR;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Common;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;

public record GetOccupantListQuery(
    Gender? Gender = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<OccupantListDto>>;