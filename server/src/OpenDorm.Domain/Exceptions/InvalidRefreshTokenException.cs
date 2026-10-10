namespace OpenDorm.Domain.Exceptions;

public class InvalidRefreshTokenException(string message) : DomainException(message);