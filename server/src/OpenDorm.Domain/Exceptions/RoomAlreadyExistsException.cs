using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Exceptions;

public class RoomAlreadyExistsException(Guid dormitoryId, RoomName name)
    : DomainException($"Room with number '{name}' already exists in dormitory with id: '{dormitoryId}'.")
{
    public Guid DormitoryId { get; } = dormitoryId;
    public RoomName Name { get; } = name;
}