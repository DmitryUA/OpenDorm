using OpenDorm.Domain.Common;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Occupant;

public class Occupant : AggregateRoot
{
    public LastName LastName { get; }
    public FirstName FirstName { get; }
    public Patronymic? Patronymic { get; }
    public string FullName => Patronymic == null ? $"{LastName} {FirstName}" : $"{LastName} {FirstName} {Patronymic}";
    public Gender Gender { get; }
    public BirthDate BirthDate { get; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Accommodation> _accommodations = [];
    
    private Occupant() {} // For EF Core only
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
        if (!IsActive) throw new DomainException($"Inactive occupant cannot be moved in. Occupant id: '{Id}'.");
            
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
    
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        if (_accommodations.Any(a => a.IsActive))
            throw new DomainException($"Cannot deactivate occupant '{FullName}' with active accommodation. Occupant id: '{Id}'.");

        IsActive = false;
    }
}