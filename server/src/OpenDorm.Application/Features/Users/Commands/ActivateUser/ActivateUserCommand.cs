using MediatR;

namespace OpenDorm.Application.Features.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid Id) : IRequest;