using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Configurations;

public class UserConfiguration(IEncryptionService encryptionService) : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", t =>
        {
            t.HasComment("пользователи");
            t.HasCheckConstraint("CK_Login_MinLength", $"LENGTH(login) >= {Login.MinLength}");
        });

        builder.HasKey(u => u.Id);
        
        builder.Property(u=>u.Id)
            .HasColumnName("id")
            .HasComment("идентификатор пользователя")
            .ValueGeneratedNever();

        builder.ComplexProperty(u => u.Login, b =>
        {
            b.Property(l => l.Value)
                .HasColumnName("login")
                .HasMaxLength(Login.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(u => u.PasswordHash, b =>
        {
            b.Property(u => u.Value)
                .HasColumnName("password_hash")
                .HasMaxLength(PasswordHash.MaxLength)
                .IsRequired();
        });
        
        builder.Property(u => u.LastName)
            .HasColumnName("last_name_encrypted")
            .HasConversion(
                lastName => encryptionService.Encrypt(Encoding.UTF8.GetBytes(lastName.Value)),
                value => new LastName(Encoding.UTF8.GetString(encryptionService.Decrypt(value))))
            .HasComment("зашифрованная фамилия")
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name_encrypted")
            .HasConversion(
                firstName => encryptionService.Encrypt(Encoding.UTF8.GetBytes(firstName.Value)),
                value => new FirstName(Encoding.UTF8.GetString(encryptionService.Decrypt(value))))
            .HasComment("зашифрованное имя")
            .IsRequired();

        builder.Property(u => u.Patronymic)
            .HasColumnName("patronymic_encrypted")
            .HasConversion(
                patronymic =>
                    patronymic == null ? null : encryptionService.Encrypt(Encoding.UTF8.GetBytes(patronymic.Value)),
                value => value == null
                    ? null
                    : new Patronymic(Encoding.UTF8.GetString(encryptionService.Decrypt(value))))
            .HasComment("зашифрованное отчество");

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}