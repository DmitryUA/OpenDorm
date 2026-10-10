using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Commands.DeactivateUser;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Unit;

public class DeactivateUserCommandHandlerUnitTests
{
    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingUserId = Guid.NewGuid();
        var command = new DeactivateUserCommand(nonExistingUserId);

        var mockRepository = Substitute.For<IUserRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();

        mockRepository
            .GetByIdAsync(nonExistingUserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        var handler = new DeactivateUserCommandHandler(mockRepository, mockUow);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(User), exception.EntityName);
        Assert.Equal(nonExistingUserId, exception.EntityId);
    }

    [Fact]
    public async Task Handle_ActiveUser_DeactivatesAndSaves()
    {
        // Arrange
        var user = UserFactory.Create();
        var command = new DeactivateUserCommand(user.Id);

        var mockRepository = Substitute.For<IUserRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();

        mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new DeactivateUserCommandHandler(mockRepository, mockUow);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(user.IsActive);
        await mockUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveUser_DoesNotChangeStateButStillSaves()
    {
        // Arrange
        var user = UserFactory.CreateInactive();
        var command = new DeactivateUserCommand(user.Id);

        var mockRepository = Substitute.For<IUserRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();

        mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new DeactivateUserCommandHandler(mockRepository, mockUow);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(user.IsActive);
        await mockUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}