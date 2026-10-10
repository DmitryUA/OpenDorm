using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Domain.Tests.EntityTests;

public class UserRoleTests
{
    [Fact]
    public void Constructor_ValidArguments_CreatesInstanceAndPreservesValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var userRole = new UserRole(id, userId, roleId);

        // Assert
        Assert.Equal(id, userRole.Id);
        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
    }
}