namespace OpenDorm.Domain.Exceptions;

public class InvalidRoomNameException(string message) : DomainException(message);