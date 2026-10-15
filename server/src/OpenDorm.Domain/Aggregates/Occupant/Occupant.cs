using OpenDorm.Domain.Common;
using OpenDorm.Domain.Common.Events;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.Occupant;

public class Occupant : AggregateRoot
{
    public LastName LastName { get; private set; } = null!;
    public FirstName FirstName { get; private set; } = null!;
    public Patronymic? Patronymic { get; private set; }
    public string FullName => Patronymic == null ? $"{LastName} {FirstName}" : $"{LastName} {FirstName} {Patronymic}";
    public Gender Gender { get; }
    public BirthDate BirthDate { get; } = null!;
    public bool IsActive { get; private set; } = true;

    private readonly List<Accommodation> _accommodations = [];
    
    // Доступно для сборки Infrastructure (Для EF Core)
    internal IReadOnlyCollection<Accommodation> Accommodations => _accommodations.AsReadOnly();
    
    // ReSharper disable once UnusedMember.Local
    private Occupant() {} // For EF Core only
    public Occupant(Guid id, LastName lastName, FirstName firstName, Patronymic? patronymic, Gender gender, BirthDate birthDate) : base(id)
    {
        LastName = lastName ?? throw new InvalidOccupantException("Occupant last name cannot be null.");
        FirstName = firstName ?? throw new InvalidOccupantException("Occupant first name cannot be null.");
        Patronymic = patronymic;
        Gender = gender;
        BirthDate = birthDate ?? throw new InvalidOccupantException("Occupant birth date cannot be null.");
    }

    public Guid CheckIn(Guid roomId)
    {
        if (!IsActive) throw new DomainException($"Inactive occupant cannot be moved in. Occupant id: '{Id}'.");
            
        if (_accommodations.Any(a => a.IsActive))
            throw new DomainException($"Occupant '{FullName}' is already living in another room. Occupant id: '{Id}'.");

        var accommodation = new Accommodation(Guid.NewGuid(), roomId, DateTime.UtcNow);
        _accommodations.Add(accommodation);

        var checkedInEvent = new OccupantCheckedInEvent(roomId, Id);
        AddDomainEvent(checkedInEvent);

        return accommodation.Id;
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

    public Guid Transfer(Guid roomId)
    {
        var isCurrentRoomIdEqualsTargetRoomId = _accommodations.Any(a => a.IsActive && a.RoomId == roomId);

        if (isCurrentRoomIdEqualsTargetRoomId)
        {
            var activeAccommodationId = _accommodations.First(a => a.IsActive).Id;
            throw new DomainException(
                $"Occupant is already living in this room. Accommodation id: '{activeAccommodationId}'.");
        }
        
        CheckOut();
        return CheckIn(roomId);
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

    public void ChangeLastName(LastName lastName) =>
        LastName = lastName ?? throw new InvalidOccupantException("Occupant last name cannot be null.");
    
    public void ChangeFirstName(FirstName firstName) =>
        FirstName = firstName ?? throw new InvalidOccupantException("Occupant first name cannot be null.");

    public void ChangePatronymic(Patronymic patronymic) =>
        Patronymic = patronymic;
}