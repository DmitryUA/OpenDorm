using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Aggregates.System;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;
using OpenDorm.Infrastructure.Configurations;

namespace OpenDorm.Infrastructure;

public class OpenDormDbContext(DbContextOptions<OpenDormDbContext> options, IEncryptionService encryptionService)
    : DbContext(options), IApplicationDbContext
{
    public IQueryable<Room> Rooms => RoomsDbSet;
    public IQueryable<Occupant> Occupants => OccupantsDbSet;
    public IQueryable<Dormitory> Dormitories => DormitoriesDbSet;
    public IQueryable<Accommodation> Accommodations => AccommodationsDbSet;
    
    public IQueryable<User> Users => UsersDbSet;
    public IQueryable<Role> Roles => RolesDbSet;
    public IQueryable<UserRole> UserRoles => UserRolesDbSet;
    public IQueryable<RefreshToken> RefreshTokens => RefreshTokensDbSet;

    // Domain
    public DbSet<Room> RoomsDbSet { get; set; }
    public DbSet<Occupant> OccupantsDbSet { get; set; }
    public DbSet<Dormitory> DormitoriesDbSet { get; set; }
    public DbSet<Accommodation> AccommodationsDbSet { get; set; }
    
    // Authorization & Authentication
    public DbSet<Role> RolesDbSet { get; set; }
    public DbSet<User> UsersDbSet { get; set; }
    public DbSet<UserRole> UserRolesDbSet { get; set; }
    public DbSet<SystemFlag> SystemFlagsDbSet { get; set; }
    public DbSet<RefreshToken> RefreshTokensDbSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new DormitoryConfiguration());
        modelBuilder.ApplyConfiguration(new AccommodationConfiguration());
        modelBuilder.ApplyConfiguration(new OccupantConfiguration(encryptionService));

        // Authorization & Authentication
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new SystemFlagConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration(encryptionService));
        
        SeedRoles(modelBuilder);
    }
    
    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        var administratorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        modelBuilder.Entity<Role>().HasData(
            new Role(administratorRoleId, RoleName.Administrator),
            new Role(managerRoleId, RoleName.Manager)
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        MarkNewEntitiesAsAdded();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void MarkNewEntitiesAsAdded()
    {
        foreach (var entry in ChangeTracker.Entries<Dormitory>())
        {
            if (entry.State is EntityState.Deleted) continue;

            foreach (var room in entry.Entity.Rooms)
            {
                var roomEntry = Entry(room);
                
                if (roomEntry.State is EntityState.Detached)
                    roomEntry.State = EntityState.Added;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Occupant>())
        {
            if (entry.State is EntityState.Deleted) continue;

            foreach (var accommodation in entry.Entity.Accommodations)
            {
                var accommodationEntry = Entry(accommodation);

                if (accommodationEntry.State is EntityState.Detached)
                    accommodationEntry.State = EntityState.Added;
            }
        }
        
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State is EntityState.Deleted) continue;
            
            foreach (var userRole in entry.Entity.UserRoles)
            {
                var userRoleEntry = Entry(userRole);
                
                if (userRoleEntry.State is EntityState.Detached)
                    userRoleEntry.State = EntityState.Added;
            }
        }
    }
}