using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.System;

namespace OpenDorm.Infrastructure.Repositories;

public class SystemFlagRepository(OpenDormDbContext dbContext) : ISystemFlagRepository
{
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.SystemFlagsDbSet
            .AnyAsync(f => f.Name == name, cancellationToken);

    public async Task AddAsync(SystemFlag flag, CancellationToken cancellationToken = default) =>
        await dbContext.SystemFlagsDbSet.AddAsync(flag, cancellationToken);
}