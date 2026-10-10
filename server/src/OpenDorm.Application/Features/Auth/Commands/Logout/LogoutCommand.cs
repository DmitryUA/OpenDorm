using MediatR;

namespace OpenDorm.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;