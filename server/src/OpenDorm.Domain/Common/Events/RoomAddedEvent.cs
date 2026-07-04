using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Common.Events;

public record RoomAddedEvent(
    Guid DormitoryId,
    Guid RoomId,
    RoomName RoomName,
    Gender Gender,
    int Capacity,
    int FloorNumber
) : DomainEvent;