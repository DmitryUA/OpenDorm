namespace OpenDorm.Domain.Exceptions;

public class InvalidPasswordHashException(string message) : DomainException(message);