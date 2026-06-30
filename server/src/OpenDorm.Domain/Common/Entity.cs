namespace OpenDorm.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity() { } // For EF Core only

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        Id = id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        
        return Id != Guid.Empty && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);
    
    public override int GetHashCode() => Id.GetHashCode();
    
    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}