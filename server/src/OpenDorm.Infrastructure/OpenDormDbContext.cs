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
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Occupant> Occupants { get; set; }
    public DbSet<Dormitory> Dormitories { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new DormitoryConfiguration());
        modelBuilder.ApplyConfiguration(new AccommodationConfiguration());
        modelBuilder.ApplyConfiguration(new OccupantConfiguration(encryptionService));
    }
}