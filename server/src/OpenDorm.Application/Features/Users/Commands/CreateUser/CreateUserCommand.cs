using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Login,
    string Password,
    string LastName,
    string FirstName,
    string? Patronymic,
    string RoleName) : IRequest<Guid>;