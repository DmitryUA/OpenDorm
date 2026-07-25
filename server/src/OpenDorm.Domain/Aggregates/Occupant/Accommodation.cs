using OpenDorm.Domain.Common;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Aggregates.Occupant;

public class Accommodation : Entity
{
    public Guid RoomId { get; }
    public DateTime CheckInDate { get; }
    public DateTime? CheckOutDate { get; private set; }
    public bool IsActive => CheckOutDate == null;

    public Accommodation(Guid id, Guid roomId, DateTime checkInDate) : base(id)
    {
        if (roomId == Guid.Empty) throw new ArgumentException("Room id cannot be empty", nameof(roomId));
        if (checkInDate == default) throw new ArgumentException("Check-in date cannot be empty.", nameof(checkInDate));
        if (checkInDate.Date > DateTime.UtcNow.Date) throw new DomainException("Cannot check-in for a feature date.");
        
        RoomId = roomId;
        CheckInDate = checkInDate;
        CheckOutDate = null;
    }

    public void CheckOut(DateTime date)
    {
        if (!IsActive) throw new DomainException("Accommodation is already completed");
        if (date < CheckInDate) throw new DomainException("Check-out date cannot be before check-in date");

        CheckOutDate = date;
    }
}