namespace OpenDorm.Domain.Common.Events;

public record OccupantCheckedOutEvent(
    Guid RoomId,
    Guid OccupantId) : DomainEvent;