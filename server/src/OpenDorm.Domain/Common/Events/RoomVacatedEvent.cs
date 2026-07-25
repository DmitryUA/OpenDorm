namespace OpenDorm.Domain.Common.Events;

public record RoomVacatedEvent(
    Guid DormitoryId,
    Guid RoomId,
    Guid OccupantId
) : DomainEvent;