using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", t => t.HasComment("роли пользователя"));

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id")
            .IsRequired();
        
        builder.HasOne<User>()
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_user_roles_users");
        
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_user_roles_roles");

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_user_roles_user_id_role_id");

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_user_roles_user_id_role_id");
    }
}