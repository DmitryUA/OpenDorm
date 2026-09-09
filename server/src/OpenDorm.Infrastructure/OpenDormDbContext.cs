using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Infrastructure.Configurations;

namespace OpenDorm.Infrastructure;

public class OpenDormDbContext(DbContextOptions<OpenDormDbContext> options, IEncryptionService encryptionService)
    : DbContext(options), IApplicationDbContext
{
    public IQueryable<Room> Rooms => RoomsDbSet;
    public IQueryable<Occupant> Occupants => OccupantsDbSet;
    public IQueryable<Dormitory> Dormitories => DormitoriesDbSet;
    public IQueryable<Accommodation> Accommodations => AccommodationsDbSet;
    
    public DbSet<Room> RoomsDbSet { get; set; }
    public DbSet<Occupant> OccupantsDbSet { get; set; }
    public DbSet<Dormitory> DormitoriesDbSet { get; set; }
    public DbSet<Accommodation> AccommodationsDbSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new DormitoryConfiguration());
        modelBuilder.ApplyConfiguration(new AccommodationConfiguration());
        modelBuilder.ApplyConfiguration(new OccupantConfiguration(encryptionService));
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
    }
}