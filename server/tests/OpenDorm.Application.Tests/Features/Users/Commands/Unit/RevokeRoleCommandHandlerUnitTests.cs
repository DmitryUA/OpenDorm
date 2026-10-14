using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Commands.RevokeRole;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Unit;

public class RevokeRoleCommandHandlerUnitTests
{
    private readonly IUserRepository _mockUserRepository;
    private readonly RevokeRoleCommandHandler _handler;

    public RevokeRoleCommandHandlerUnitTests()
    {
        var mockContext = Substitute.For<IApplicationDbContext>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();
        _handler = new RevokeRoleCommandHandler(mockContext, _mockUserRepository, mockUow);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingUserId = Guid.NewGuid();
        var command = new RevokeRoleCommand(nonExistingUserId, "Administrator");

        _mockUserRepository
            .GetByIdAsync(nonExistingUserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(User), exception.EntityName);
        Assert.Equal(nonExistingUserId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_InvalidRoleName_ThrowsNotFoundException()
    {
        // Arrange
        var user = UserFactory.Create();
        var command = new RevokeRoleCommand(user.Id, "InvalidRoleName123");

        _mockUserRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(Role), exception.EntityName);
        Assert.Equal("InvalidRoleName123", exception.EntityId);
    }
}