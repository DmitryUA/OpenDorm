using OpenDorm.Domain.Common;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Occupant;

public class Occupant : AggregateRoot
{
    public LastName LastName { get; init; }
    public FirstName FirstName { get; init; }
    public Patronymic? Patronymic { get; init; }
    public string FullName => Patronymic == null ? $"{LastName} {FirstName}" : $"{LastName} {FirstName} {Patronymic}";
    public Gender Gender { get; init; }
    public BirthDate BirthDate { get; init; }

    private readonly List<Accommodation> _accommodations = [];
    
    public Occupant(Guid id, LastName lastName, FirstName firstName, Patronymic? patronymic, Gender gender, BirthDate birthDate) : base(id)
    {
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(birthDate);
        
        LastName = lastName;
        FirstName = firstName;
        Patronymic = patronymic;
        Gender = gender;
        BirthDate = birthDate;
    }

    public void CheckIn(Guid roomId)
    {
        if (_accommodations.Any(a => a.IsActive))
            throw new DomainException($"'{FullName}' is already living in another room. Occupant id: '{Id}'");

        var accommodation = new Accommodation(Guid.NewGuid(), roomId, DateTime.UtcNow);
        _accommodations.Add(accommodation);

        var checkedInEvent = new OccupantCheckedInEvent(roomId, Id);
        AddDomainEvent(checkedInEvent);
    }
    
    public void CheckOut()
    {
        var activeAccommodation = _accommodations.FirstOrDefault(a => a.IsActive);

        if (activeAccommodation == null)
            throw new DomainException($"'{FullName}' does not live in any of the rooms. Occupant id: '{Id}'");

        var checkedOutEvent = new OccupantCheckedOutEvent(activeAccommodation.RoomId, Id);
        AddDomainEvent(checkedOutEvent);
        
        activeAccommodation.CheckOut(DateTime.UtcNow);
    }
}