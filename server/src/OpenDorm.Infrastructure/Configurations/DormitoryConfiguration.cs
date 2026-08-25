using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Configurations;

public class DormitoryConfiguration : IEntityTypeConfiguration<Dormitory>
{
    private const string TableName = "dormitories";
    public void Configure(EntityTypeBuilder<Dormitory> builder)
    {
        builder.ToTable(TableName, tb => tb.HasComment("общежития"));

        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasComment("идентификатор общежития");

        builder.Property(d => d.FloorCount)
            .HasColumnName("floor_count")
            .HasComment("общее количество этажей в здании")
            .IsRequired();

        builder.OwnsOne(d => d.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.City)
                .HasColumnName("address_city")
                .HasConversion(
                    city => city.Value,
                    value => new City(value))
                .HasComment("город")
                .HasMaxLength(City.MaxLength)
                .IsRequired();

            addressBuilder.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasConversion(
                    street => street.Value,
                    value => new Street(value))
                .HasComment("улица")
                .HasMaxLength(Street.MaxLength)
                .IsRequired();

            addressBuilder.Property(a => a.House)
                .HasColumnName("address_house")
                .HasConversion(
                    house => house.Value,
                    value => new HouseNumber(value))
                .HasComment("номер дома")
                .HasMaxLength(HouseNumber.MaxLength)
                .IsRequired();
            
            addressBuilder.ToTable(TableName, t =>
            {
                t.HasCheckConstraint(
                    "CK_Address_City_MinLength",
                    $"LENGTH(address_city) >= {City.MinLength}");
                t.HasCheckConstraint(
                    "CK_Address_Street_MinLength",
                    $"LENGTH(address_street) >= {Street.MinLength}");
            });
        });
        
        builder.HasMany<Room>("_rooms")
            .WithOne()
            .HasForeignKey("DormitoryId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}