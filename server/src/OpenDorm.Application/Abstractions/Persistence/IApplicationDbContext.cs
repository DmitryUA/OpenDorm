using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    IQueryable<Dormitory> Dormitories { get; }
    IQueryable<Room> Rooms { get; }
    IQueryable<Occupant> Occupants { get; }
    IQueryable<Accommodation> Accommodations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}