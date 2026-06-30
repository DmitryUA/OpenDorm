using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Exceptions;

public class DuplicateOccupantException(Guid occupantId, Guid roomId, RoomName name)
    : DomainException($"Occupant with id '{occupantId}' is already in room '{name}'. Room id: '{roomId}'")
{
    public Guid RoomId { get; } = roomId;
    public RoomName RoomName { get; } = name;
    public Guid OccupantId { get; } = occupantId;
}