namespace OpenDorm.Domain.Common.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccuredOn { get; }
}