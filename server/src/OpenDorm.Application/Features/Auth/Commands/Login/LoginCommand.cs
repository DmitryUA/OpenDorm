using MediatR;

namespace OpenDorm.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Login, string Password) : IRequest<LoginResponse>;