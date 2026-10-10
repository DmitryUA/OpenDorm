using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Commands.UpdateUser;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;
using OpenDorm.Shared.Tests.Factories;

namespace OpenDorm.Application.Tests.Features.Users.Commands.Unit;

public class UpdateUserCommandHandlerUnitTests
{
    private readonly IUserRepository _mockRepository;
    private readonly IPasswordHasher _mockPasswordHasher;
    private readonly IUnitOfWork _mockUow;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerUnitTests()
    {
        _mockRepository = Substitute.For<IUserRepository>();
        _mockPasswordHasher = Substitute.For<IPasswordHasher>();
        _mockUow = Substitute.For<IUnitOfWork>();
        _handler = new UpdateUserCommandHandler(_mockRepository, _mockPasswordHasher, _mockUow);
    }

    #region User Not Found

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingUserId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            nonExistingUserId,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(nonExistingUserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(nameof(User), exception.EntityName);
        Assert.Equal(nonExistingUserId, exception.EntityId);
    }

    #endregion

    #region Personal Data Update

    [Fact]
    public async Task Handle_BothLastNameAndFirstNameProvided_UpdatesPersonalData()
    {
        // Arrange
        var user = UserFactory.Create();
        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: "Петрович",
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Петров", user.LastName.Value);
        Assert.Equal("Пётр", user.FirstName.Value);
        Assert.Equal("Петрович", user.Patronymic!.Value);
    }

    [Fact]
    public async Task Handle_LastNameAndFirstNameWithoutPatronymic_SetsPatronymicToNull()
    {
        // Arrange
        var user = UserFactory.Create();
        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Петров", user.LastName.Value);
        Assert.Equal("Пётр", user.FirstName.Value);
        Assert.Null(user.Patronymic);
    }

    [Fact]
    public async Task Handle_OnlyLastNameProvided_DoesNotUpdatePersonalData()
    {
        // Arrange
        var user = UserFactory.Create();
        var originalLastName = user.LastName.Value;
        var originalFirstName = user.FirstName.Value;

        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: null,
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — контракт: обновление только если оба поля не null
        Assert.Equal(originalLastName, user.LastName.Value);
        Assert.Equal(originalFirstName, user.FirstName.Value);
    }

    [Fact]
    public async Task Handle_OnlyFirstNameProvided_DoesNotUpdatePersonalData()
    {
        // Arrange
        var user = UserFactory.Create();
        var originalLastName = user.LastName.Value;
        var originalFirstName = user.FirstName.Value;

        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: "Пётр",
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalLastName, user.LastName.Value);
        Assert.Equal(originalFirstName, user.FirstName.Value);
    }

    #endregion

    #region Password Change

    [Fact]
    public async Task Handle_NewPasswordProvided_ChangesPassword()
    {
        // Arrange
        var user = UserFactory.Create();
        const string newPassword = "NewPassword123!";
        var newHash = new PasswordHash("$2a$12$new2hash");

        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: null,
            Patronymic: null,
            NewPassword: newPassword);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockPasswordHasher.Hash(newPassword).Returns(newHash);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newHash, user.PasswordHash);
        _mockPasswordHasher.Received(1).Hash(newPassword);
    }

    [Fact]
    public async Task Handle_NewPasswordNotProvided_DoesNotChangePassword()
    {
        // Arrange
        var user = UserFactory.Create();
        var originalHash = user.PasswordHash;

        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: null,
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalHash, user.PasswordHash);
        _mockPasswordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    #endregion

    #region Combined Updates

    [Fact]
    public async Task Handle_BothPersonalDataAndPasswordProvided_UpdatesBoth()
    {
        // Arrange
        var user = UserFactory.Create();
        const string newPassword = "NewPassword123!";
        var newHash = new PasswordHash("$2a$12$new2hash");

        var command = new UpdateUserCommand(
            user.Id,
            LastName: "Петров",
            FirstName: "Пётр",
            Patronymic: "Петрович",
            NewPassword: newPassword);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockPasswordHasher.Hash(newPassword).Returns(newHash);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Петров", user.LastName.Value);
        Assert.Equal("Пётр", user.FirstName.Value);
        Assert.Equal("Петрович", user.Patronymic!.Value);
        Assert.Equal(newHash, user.PasswordHash);
    }

    #endregion

    #region SaveChanges

    [Fact]
    public async Task Handle_AlwaysCallsSaveChanges()
    {
        // Arrange
        var user = UserFactory.Create();
        var command = new UpdateUserCommand(
            user.Id,
            LastName: null,
            FirstName: null,
            Patronymic: null,
            NewPassword: null);

        _mockRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mockUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}