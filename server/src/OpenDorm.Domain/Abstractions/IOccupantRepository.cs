using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Domain.Abstractions;

public interface IOccupantRepository
{
    Task AddAsync(Occupant occupant);
    Task<Occupant?> GetByIdAsync(Guid id, CancellationToken ct = default);
}