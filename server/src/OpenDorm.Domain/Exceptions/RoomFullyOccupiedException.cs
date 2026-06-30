using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Exceptions;

public class RoomFullyOccupiedException(Guid roomId, RoomName name)
    : DomainException($"Room '{name}' is fully booked. Room id: '{roomId}'")
{
    public Guid RoomId { get; } = roomId;
    public RoomName Name { get; } = name;
}