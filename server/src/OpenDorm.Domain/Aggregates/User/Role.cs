using OpenDorm.Domain.Common;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Aggregates.User;

public class Role : Entity
{
    public const int MaxLength = 64;
    public RoleName Name { get; private set; }
    public string NormalizedName { get; private set; } = null!;
    
    // ReSharper disable once UnusedMember.Local
    private Role() { } // For EF Core only

    public Role(Guid id, RoleName name) : base(id)
    {
        Name = name;
        NormalizedName = name.ToString().ToUpperInvariant();

        if (NormalizedName.Length > MaxLength)
            throw new InvalidRoleException($"Normalized role name cannot exceed {MaxLength} characters.");
    }
}