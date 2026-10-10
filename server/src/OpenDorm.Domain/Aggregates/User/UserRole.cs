using OpenDorm.Domain.Common;

namespace OpenDorm.Domain.Aggregates.User;

public class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    
    // ReSharper disable once UnusedMember.Local
    private UserRole() { } // For EF Core only

    public UserRole(Guid id, Guid userId, Guid roleId) : base(id)
    {
        UserId = userId;
        RoleId = roleId;
    }
}