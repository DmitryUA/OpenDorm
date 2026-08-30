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
}