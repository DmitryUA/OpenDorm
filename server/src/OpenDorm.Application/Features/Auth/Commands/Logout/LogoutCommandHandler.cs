using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork uow) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken)
                    ?? throw new NotFoundException(nameof(RefreshToken), command.RefreshToken);

        await refreshTokenRepository.RevokeAsync(token, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }
}