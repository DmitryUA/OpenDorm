using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    IQueryable<Dormitory> Dormitories { get; }
    IQueryable<Room> Rooms { get; }
    IQueryable<Occupant> Occupants { get; }
    IQueryable<Accommodation> Accommodations { get; }
    
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}