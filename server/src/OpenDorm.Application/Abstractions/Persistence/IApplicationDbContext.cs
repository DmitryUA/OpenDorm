using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Dormitory> Dormitories { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Occupant> Occupants { get; }
    DbSet<Accommodation> Accommodations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}