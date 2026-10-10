using OpenDorm.Domain.Aggregates.System;

namespace OpenDorm.Domain.Abstractions;

public interface ISystemFlagRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(SystemFlag flag, CancellationToken cancellationToken = default);
}