namespace OpenDorm.Domain.Common.Events;

public record OccupantCheckedOutEvent(
    Guid DormitoryId,
    Guid RoomId,
    Guid OccupantId
) : DomainEvent;