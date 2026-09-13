using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetails;

public class GetDormitoryDetailsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetDormitoryDetailsQuery, DormitoryDetailsDto>
{
    public async Task<DormitoryDetailsDto> Handle(
        GetDormitoryDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await (
            from d in dbContext.Dormitories
            where d.Id == request.Id
            let roomCount = dbContext.Rooms
                .Count(r => EF.Property<Guid>(r, "DormitoryId") == d.Id)
            let totalSeats = dbContext.Rooms
                .Where(r => EF.Property<Guid>(r, "DormitoryId") == d.Id)
                .Sum(r => (int?)r.Capacity) ?? 0
            let occupiedSeats = dbContext.Accommodations
                .Count(a => a.CheckOutDate == null
                            && dbContext.Rooms.Any(r => r.Id == a.RoomId
                                                        && EF.Property<Guid>(r, "DormitoryId") == d.Id))
            select new DormitoryDetailsDto(
                d.Id,
                d.Address.City.Value,
                d.Address.Street.Value,
                d.Address.House.Value,
                roomCount,
                totalSeats,
                d.FloorCount,
                totalSeats - occupiedSeats
            )
        ).FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Dormitory), request.Id);
    }
}