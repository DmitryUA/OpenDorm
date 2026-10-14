using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", t => t.HasComment("токены обновления"));

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .HasComment("идентификатор токена")
            .ValueGeneratedNever();

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(RefreshToken.MaxLength)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_refresh_tokens_users");

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_refresh_tokens_token");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_refresh_tokens_token");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_refresh_tokens_user_id");
    }
}