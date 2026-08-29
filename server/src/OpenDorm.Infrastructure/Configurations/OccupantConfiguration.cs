using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Configurations;

public class OccupantConfiguration(IEncryptionService encryptionService) : IEntityTypeConfiguration<Occupant>
{
    private const string TableName = "occupants";

    public void Configure(EntityTypeBuilder<Occupant> builder)
    {
        builder.ToTable(TableName, t => t.HasComment("жильцы"));

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasComment("идентификатор жильца");

        builder.Property(o => o.LastName)
            .HasColumnName("last_name_encrypted")
            .HasConversion(
                lastName => encryptionService.Encrypt(Encoding.UTF8.GetBytes(lastName.Value)),
                value => new LastName(Encoding.UTF8.GetString(encryptionService.Decrypt(value))))
            .HasComment("зашифрованная фамилия")
            .HasMaxLength(LastName.MaxLength)
            .IsRequired();

        builder.Property(o => o.FirstName)
            .HasColumnName("first_name_encrypted")
            .HasConversion(
                firstName => encryptionService.Encrypt(Encoding.UTF8.GetBytes(firstName.Value)),
                value => new FirstName(Encoding.UTF8.GetString(encryptionService.Decrypt(value))))
            .HasComment("зашифрованное имя")
            .HasMaxLength(FirstName.MaxLength)
            .IsRequired();

        builder.Property(o => o.Patronymic)
            .HasColumnName("patronymic_encrypted")
            .HasConversion(
                patronymic => patronymic != null ? encryptionService.Encrypt(Encoding.UTF8.GetBytes(patronymic.Value)) : null,
                value => value != null ? new Patronymic(Encoding.UTF8.GetString(encryptionService.Decrypt(value))) : null
            )
            .HasComment("зашифрованное отчество")
            .HasMaxLength(Patronymic.MaxLength)
            .IsRequired(false);

        builder.Property(o => o.Gender)
            .HasColumnName("gender")
            .HasConversion(
                gender => gender == Gender.Male ? 'm' : 'f',
                value => value == 'm' ? Gender.Male : Gender.Female)
            .HasComment("гендер (m - мужчина, f - женщина)")
            .IsRequired();

        builder.Property(o => o.BirthDate)
            .HasColumnName("birth_date_encrypted")
            .HasConversion(
                birthDate => encryptionService.Encrypt(Encoding.UTF8.GetBytes(birthDate.Value.ToString("O"))),
                value => new BirthDate(DateOnly.Parse(Encoding.UTF8.GetString(encryptionService.Decrypt(value)))))
            .HasComment("зашифрованная дата рождения")
            .IsRequired();

        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .HasComment("активен ли проживающий, true - да, false - нет")
            .IsRequired();

        builder.HasMany<Accommodation>("_accommodations")
            .WithOne()
            .HasForeignKey("OccupantId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}