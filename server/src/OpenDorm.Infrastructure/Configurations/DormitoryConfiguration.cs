using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.Dormitory;

namespace OpenDorm.Infrastructure.Configurations;

public class DormitoryConfiguration : IEntityTypeConfiguration<Dormitory>
{
    public void Configure(EntityTypeBuilder<Dormitory> builder)
    {
        builder.ToTable("dormitories", tb => tb.HasComment("общежития"));

        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.FloorCount)
            .HasColumnName("floor_count")
            .HasComment("общее количество этажей в здании")
            .IsRequired();

        builder.OwnsOne(d => d.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.City)
                .HasColumnName("address_city")
                .HasComment("город")
                .HasMaxLength(50)
                .IsRequired();

            addressBuilder.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasComment("улица")
                .HasMaxLength(255)
                .IsRequired();

            addressBuilder.Property(a => a.House)
                .HasColumnName("address_house")
                .HasComment("номер дома")
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}