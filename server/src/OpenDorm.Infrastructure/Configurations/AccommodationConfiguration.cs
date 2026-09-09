using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Infrastructure.Configurations;

public class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
{
    private const string TableName = "accommodations";
    
    public void Configure(EntityTypeBuilder<Accommodation> builder)
    {
        builder.ToTable(TableName, t => t.HasComment("заселения"));

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasComment("идентификатор заселения")
            .ValueGeneratedNever();
        
        builder.Property(a => a.CheckInDate)
            .HasColumnName("check_in_date")
            .HasComment("дата и время заселения")
            .IsRequired();

        builder.Property(a => a.CheckOutDate)
            .HasColumnName("check_out_date")
            .HasComment("дата и время выселения")
            .IsRequired(false);

        builder.Property<Guid>("OccupantId")
            .HasColumnName("occupant_id")
            .HasComment("идентификатор жильца");

        builder.Property<Guid>("RoomId")
            .HasColumnName("room_id")
            .HasComment("идентификатор комнаты");
            
        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey("RoomId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("OccupantId")
            .HasFilter("check_out_date IS NULL")
            .HasDatabaseName("UX_Accommodations_OccupantId_Active");
    }
}