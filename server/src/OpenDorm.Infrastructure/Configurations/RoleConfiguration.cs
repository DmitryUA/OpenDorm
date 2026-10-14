using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", t => t.HasComment("роли"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasComment("идентификатор роли")
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(r => r.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(Role.MaxLength)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_roles_name");
    }
}