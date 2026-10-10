using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Common;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken)
            ?? throw new DomainException("Невалидный refresh token.");

        if (!existingToken.IsValid)
            throw new DomainException("Refresh token истёк или отозван.");
        
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken)
            ?? throw new DomainException("Пользователь не найден.");

        if (!user.IsActive)
            throw new DomainException("Пользователь деактивирован.");
        
        var roleNames = await context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Join(
                context.Roles.AsNoTracking(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync(cancellationToken);

        if (roleNames.Count == 0)
            throw new DomainException("У пользователя нет назначенных ролей.");
        
        var permissions = roleNames
            .SelectMany(r => RolePermissions.GetPermissions(r))
            .Distinct()
            .ToList();

        var tokenResult = jwtTokenGenerator.GenerateTokens(
            user.Id,
            user.Login.Value,
            roleNames.Select(r => r.ToString()),
            permissions);

        // Ротация: отзываем старый refresh token
        var trackedToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token не найден.");

        await refreshTokenRepository.RevokeAsync(trackedToken, cancellationToken);
        
        var newRefreshToken = new Domain.Aggregates.User.RefreshToken(
            Guid.NewGuid(),
            user.Id,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);
    }
}