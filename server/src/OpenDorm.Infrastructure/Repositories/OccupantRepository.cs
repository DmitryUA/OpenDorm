using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Infrastructure.Repositories;

public class OccupantRepository(OpenDormDbContext dbContext) : IOccupantRepository
{
    public async Task AddAsync(Occupant occupant) => await dbContext.Occupants.AddAsync(occupant);

    public async Task<Occupant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Occupants.FirstOrDefaultAsync(o => o.Id == id, ct);
}