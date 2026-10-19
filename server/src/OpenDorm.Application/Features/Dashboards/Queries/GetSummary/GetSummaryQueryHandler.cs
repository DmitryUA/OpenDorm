using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;

namespace OpenDorm.Application.Features.Dashboards.Queries.GetSummary;

public class GetSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSummaryQuery, SummaryDto>
{
    public async Task<SummaryDto> Handle(GetSummaryQuery query, CancellationToken cancellationToken)
    {
        return await context.Dormitories
            .Select(_ => 1)
            .DefaultIfEmpty()
            .Select(_ => new SummaryDto(
                context.Dormitories.Count(),
                context.Rooms.Count(room => room.IsActive),
                context.Accommodations.Count(),
                context.Occupants.Count(occupant => occupant.IsActive)))
            .FirstAsync(cancellationToken);
    }
}