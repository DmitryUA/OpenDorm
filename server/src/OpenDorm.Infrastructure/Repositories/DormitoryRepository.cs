using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;

namespace OpenDorm.Infrastructure.Repositories;

public class DormitoryRepository(OpenDormDbContext dbContext) : IDormitoryRepository
{
    public async Task AddAsync(Dormitory dormitory)
    {
        await dbContext.Dormitories.AddAsync(dormitory);
    }

    public async Task<Dormitory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Dormitories.FirstOrDefaultAsync(d => d.Id == id, ct);
    }
}