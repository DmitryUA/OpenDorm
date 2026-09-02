namespace OpenDorm.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public string EntityName { get; } = string.Empty;
    public object? EntityId { get; } = null;
    
    public NotFoundException(string entityName, object? entityId = null)
        : base(BuildMessage(entityName, entityId))
    {
        EntityName = entityName;
        EntityId = entityId;
    }
    
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    private static string BuildMessage(string entityName, object? entityId)
        => entityId is null
            ? $"{entityName} not found."
            : $"{entityName} with id '{entityId}' not found.";
}