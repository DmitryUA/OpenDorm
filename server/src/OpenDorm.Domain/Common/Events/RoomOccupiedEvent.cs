namespace OpenDorm.Domain.Common.Events;

public record RoomOccupiedEvent(
    Guid DormitoryId,
    Guid RoomId,
    Guid OccupantId
) : DomainEvent;