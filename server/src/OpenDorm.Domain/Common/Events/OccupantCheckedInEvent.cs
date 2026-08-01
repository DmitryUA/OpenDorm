namespace OpenDorm.Domain.Common.Events;

public record OccupantCheckedInEvent(
    Guid RoomId,
    Guid OccupantId ):DomainEvent;