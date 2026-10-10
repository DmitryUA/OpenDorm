using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Shared.Tests.Factories;

public static class UserFactory
{
    /// <summary>
    /// Создаёт валидного пользователя с указанными параметрами.
    /// Параметры, не переданные явно, заполняются дефолтными валидными значениями.
    /// </summary>
    public static User Create(
        Login? login = null,
        PasswordHash? passwordHash = null,
        LastName? lastName = null,
        FirstName? firstName = null,
        Patronymic? patronymic = null)
    {
        return new User(
            Guid.NewGuid(),
            login ?? new Login("ivanov1984"),
            passwordHash ?? new PasswordHash("$2a$12$hash"),
            lastName ?? new LastName("Иванов"),
            firstName ?? new FirstName("Иван"),
            patronymic ?? new Patronymic("Иванович"));
    }

    /// <summary>
    /// Создаёт валидного пользователя без отчества.
    /// </summary>
    public static User CreateWithoutPatronymic(
        Login? login = null,
        PasswordHash? passwordHash = null,
        LastName? lastName = null,
        FirstName? firstName = null)
    {
        return new User(
            Guid.NewGuid(),
            login ?? new Login("ivanov1984"),
            passwordHash ?? new PasswordHash("$2a$12$hash"),
            lastName ?? new LastName("Иванов"),
            firstName ?? new FirstName("Иван"),
            null);
    }

    /// <summary>
    /// Создаёт деактивированного пользователя.
    /// </summary>
    public static User CreateInactive()
    {
        var user = Create();
        user.Deactivate();
        return user;
    }
}