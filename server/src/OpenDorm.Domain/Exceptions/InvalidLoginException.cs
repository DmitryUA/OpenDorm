namespace OpenDorm.Domain.Exceptions;

public class InvalidLoginException(string message) : DomainException(message);