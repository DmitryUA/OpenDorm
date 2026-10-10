using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string? LastName,
    string? FirstName,
    string? Patronymic,
    string? NewPassword) : IRequest;