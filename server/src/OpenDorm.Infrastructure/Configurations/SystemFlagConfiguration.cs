using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.System;

namespace OpenDorm.Infrastructure.Configurations;

public class SystemFlagConfiguration : IEntityTypeConfiguration<SystemFlag>
{
    public void Configure(EntityTypeBuilder<SystemFlag> builder)
    {
        builder.ToTable("system_flags");

        builder.HasKey(f => f.Id);
        
        builder.Property(f=>f.Id)
            .HasColumnName("id")
            .HasComment("идентификатор флага")
            .ValueGeneratedNever();

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .HasMaxLength(SystemFlag.MaxNameLength)
            .IsRequired();

        builder.Property(f => f.Value)
            .HasColumnName("value")
            .HasMaxLength(SystemFlag.MaxValueLength)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(f => f.Name)
            .IsUnique()
            .HasDatabaseName("IX_system_flags_name");
    }
}