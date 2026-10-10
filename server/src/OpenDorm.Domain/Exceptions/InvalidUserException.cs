namespace OpenDorm.Domain.Exceptions;

public class InvalidUserException(string message) : DomainException(message);