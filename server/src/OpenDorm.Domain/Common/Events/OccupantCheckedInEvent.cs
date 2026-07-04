namespace OpenDorm.Domain.Common.Events;

public record OccupantCheckedInEvent(
    Guid DormitoryId,
    Guid RoomId,
    Guid OccupantId
) : DomainEvent;