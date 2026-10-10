using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Domain.Tests.EntityTests;

public class RoleTests
{
    [Theory]
    [InlineData(RoleName.Administrator)]
    [InlineData(RoleName.Manager)]
    public void Constructor_ValidArguments_CreatesInstanceAndPreservesValues(RoleName roleName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var role = new Role(id, roleName);

        // Assert
        Assert.Equal(id, role.Id);
        Assert.Equal(roleName, role.Name);
    }

    [Theory]
    [InlineData(RoleName.Administrator, "ADMINISTRATOR")]
    [InlineData(RoleName.Manager, "MANAGER")]
    public void Constructor_ValidArguments_SetsNormalizedNameToUpperCase(RoleName roleName, string expectedNormalizedName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var role = new Role(id, roleName);

        // Assert
        Assert.Equal(expectedNormalizedName, role.NormalizedName);
    }
}