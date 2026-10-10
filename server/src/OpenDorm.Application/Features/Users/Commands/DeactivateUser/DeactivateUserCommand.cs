using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid Id) : IRequest;