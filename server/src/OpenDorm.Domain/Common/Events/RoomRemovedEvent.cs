namespace OpenDorm.Domain.Common.Events;

public record RoomRemovedEvent(
    Guid DormitoryId,
    Guid RoomId
) : DomainEvent;