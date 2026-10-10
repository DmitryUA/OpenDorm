using OpenDorm.Domain.Common;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Domain.Aggregates.User;

public class Role : Entity
{
    public RoleName Name { get; private set; }
    public string NormalizedName { get; private set; } = null!;
    
    // ReSharper disable once UnusedMember.Local
    private Role() { } // For EF Core only

    public Role(Guid id, RoleName name) : base(id)
    {
        Name = name;
        NormalizedName = name.ToString().ToUpperInvariant();
    }
}