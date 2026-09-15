using OpenDorm.Domain.Aggregates.Occupant;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Shared.Tests.Factories;

public static class OccupantFactory
{
    private static int _counter = 1;

    /// <summary>
    /// Создаёт жильца без заселений.
    /// </summary>
    public static Occupant Create(
        string? lastName = null,
        string? firstName = null,
        string? patronymic = null,
        Gender gender = Gender.Male,
        DateOnly? birthDate = null)
    {
        var last = lastName ?? "Фамилия";
        var first = firstName ?? "Имя";
        _counter++;

        return new Occupant(
            Guid.NewGuid(),
            new LastName(last),
            new FirstName(first),
            patronymic is null ? null : new Patronymic(patronymic),
            gender,
            new BirthDate(birthDate ?? new DateOnly(2000, 1, 1)));
    }

    /// <summary>
    /// Создаёт жильца с одним активным заселением в указанную комнату.
    /// Возвращает кортеж (жилец, Id активного заселения).
    /// </summary>
    public static (Occupant Occupant, Guid AccommodationId) CreateCheckedIn(
        Guid roomId,
        string? lastName = null,
        string? firstName = null,
        Gender gender = Gender.Male)
    {
        var occupant = Create(lastName, firstName, gender: gender);
        var accommodationId = occupant.CheckIn(roomId);
        return (occupant, accommodationId);
    }

    /// <summary>
    /// Создаёт жильца с историей заселений (последнее — активное).
    /// Возвращает кортеж (жилец, список Id всех заселений, Id активного).
    /// </summary>
    public static (Occupant Occupant, IReadOnlyList<Guid> AllAccommodationIds, Guid ActiveAccommodationId) CreateWithHistory(
        IReadOnlyList<Guid> roomIds)
    {
        if (roomIds == null || roomIds.Count == 0)
            throw new ArgumentException("At least one room id is required", nameof(roomIds));

        var occupant = Create();
        var allIds = new List<Guid>(roomIds.Count);

        for (var i = 0; i < roomIds.Count; i++)
        {
            var accId = occupant.CheckIn(roomIds[i]);
            allIds.Add(accId);

            // Все заселения, кроме последнего, нужно выселить
            if (i < roomIds.Count - 1) occupant.CheckOut();
        }

        return (occupant, allIds, allIds[^1]);
    }
}