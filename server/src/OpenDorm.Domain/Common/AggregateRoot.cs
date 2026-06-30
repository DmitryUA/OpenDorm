namespace OpenDorm.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private AggregateRoot() : base() { } // For EF Core only

    protected AggregateRoot(Guid id) : base(id) { }
}