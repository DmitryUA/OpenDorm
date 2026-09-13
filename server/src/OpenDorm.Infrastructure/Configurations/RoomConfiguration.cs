using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms", t =>
        {
            t.HasComment("комнаты в общежитиях");
            t.HasCheckConstraint("CK_Name_MinLength", $"LENGTH(name) >= {RoomName.MinLength}");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasComment("идентификатор комнаты")
            .ValueGeneratedNever();

        builder.Property(r => r.Capacity)
            .HasColumnName("capacity")
            .HasComment("вместимость")
            .IsRequired();

        builder.ComplexProperty(r => r.Name, b =>
        {
            b.Property(n => n.Value)
                .HasColumnName("name")
                .HasComment("номер комнаты")
                .HasMaxLength(RoomName.MaxLength)
                .IsRequired();
        });

        builder.Property(r => r.Gender)
            .HasColumnName("gender")
            .HasConversion(
                gender => gender == Gender.Male ? 'm' : 'f',
                value => value == 'm' ? Gender.Male : Gender.Female)
            .HasComment("гендер, m - мужская, f - женская")
            .IsRequired();

        builder.Property(r => r.FloorNumber)
            .HasColumnName("floor_number")
            .HasComment("номер этажа")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasComment("активна ли комната")
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property<Guid>("DormitoryId")
            .HasColumnName("dormitory_id")
            .HasComment("идентификатор общежития");
    }
}