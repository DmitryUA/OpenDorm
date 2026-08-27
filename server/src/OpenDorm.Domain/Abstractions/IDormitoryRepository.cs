using OpenDorm.Domain.Aggregates.Dormitory;

namespace OpenDorm.Domain.Abstractions;

public interface IDormitoryRepository
{
    Task AddAsync(Dormitory dormitory);
    Task<Dormitory?> GetByIdAsync(Guid id, CancellationToken ct = default);
}