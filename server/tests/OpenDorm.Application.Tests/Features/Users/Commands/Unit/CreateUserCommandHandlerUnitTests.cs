using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Commands.CreateUser;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Unit;

public class CreateUserCommandHandlerUnitTests
{
    private readonly IUserRepository _mockUserRepository;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerUnitTests()
    {
        var mockContext = Substitute.For<IApplicationDbContext>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        var mockPasswordHasher = Substitute.For<IPasswordHasher>();
        var mockUow = Substitute.For<IUnitOfWork>();
        _handler = new CreateUserCommandHandler(
            mockContext, _mockUserRepository, mockPasswordHasher, mockUow);
    }

    [Fact]
    public async Task Handle_ExistingLogin_ThrowsDomainException()
    {
        // Arrange
        const string existingLogin = "ivanov1984";
        var command = new CreateUserCommand(
            existingLogin,
            "Password123!",
            "Иванов",
            "Иван",
            "Иванович",
            "Administrator");

        _mockUserRepository
            .ExistsByLoginAsync(Arg.Any<Login>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains(existingLogin, exception.Message);
    }
}