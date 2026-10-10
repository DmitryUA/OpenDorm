using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest;