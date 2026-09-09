using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Infrastructure.Repositories;

public class OccupantRepository(OpenDormDbContext dbContext) : IOccupantRepository
{
    public async Task AddAsync(Occupant occupant) => await dbContext.OccupantsDbSet.AddAsync(occupant);

    public async Task<Occupant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.OccupantsDbSet
            .Include("_accommodations")
            .FirstOrDefaultAsync(o => o.Id == id, ct);
}