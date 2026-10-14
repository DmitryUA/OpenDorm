namespace OpenDorm.Domain.Exceptions;

public class InvalidRoomCapacityException(string message) : DomainException(message);