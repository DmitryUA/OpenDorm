using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Commands.AssignRole;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Unit;

public class AssignRoleCommandHandlerUnitTests
{
    private readonly IUserRepository _mockUserRepository;
    private readonly AssignRoleCommandHandler _handler;

    public AssignRoleCommandHandlerUnitTests()
    {
        var mockContext = Substitute.For<IApplicationDbContext>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();
        _handler = new AssignRoleCommandHandler(mockContext, _mockUserRepository, mockUow);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingUserId = Guid.NewGuid();
        var command = new AssignRoleCommand(nonExistingUserId, "Administrator");

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
        var command = new AssignRoleCommand(user.Id, "InvalidRoleName123");

        _mockUserRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}