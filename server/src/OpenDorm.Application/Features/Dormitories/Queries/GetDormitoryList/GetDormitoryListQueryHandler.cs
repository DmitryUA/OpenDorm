using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;

public class GetDormitoryListQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetDormitoryListQuery, IReadOnlyCollection<DormitoryListDto>>
{
    public async Task<IReadOnlyCollection<DormitoryListDto>> Handle(
        GetDormitoryListQuery request,
        CancellationToken cancellationToken)
    {
        var dormitories = await dbContext.Dormitories
            .AsNoTracking()
            .Select(d => new DormitoryListDto(
                d.Id,
                d.Address.City.Value,
                d.Address.Street.Value,
                d.Address.House.Value,
                d.FloorCount))
            .ToListAsync(cancellationToken);

        return
        [
            .. dormitories.OrderBy(d => d.City)
                .ThenBy(d => d.Street)
        ];
    }
}