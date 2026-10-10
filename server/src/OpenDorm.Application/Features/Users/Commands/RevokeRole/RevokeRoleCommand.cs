using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.RevokeRole;

public record RevokeRoleCommand(Guid UserId, string RoleName) : IRequest;