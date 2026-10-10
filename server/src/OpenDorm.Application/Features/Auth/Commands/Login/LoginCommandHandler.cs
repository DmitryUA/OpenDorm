using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Common;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext context,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork uow) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var login = new Domain.ValueObjects.Login(command.Login);
        
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login.Value == login.Value, cancellationToken)
            ?? throw new DomainException("Неверный логин или пароль.");

        if (!user.IsActive)
            throw new DomainException("Пользователь деактивирован.");

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new DomainException("Неверный логин или пароль.");
        
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
            .SelectMany(RolePermissions.GetPermissions)
            .Distinct()
            .ToList();

        var tokenResult = jwtTokenGenerator.GenerateTokens(
            user.Id,
            user.Login.Value,
            roleNames.Select(r => r.ToString()),
            permissions);
        
        var refreshToken = new Domain.Aggregates.User.RefreshToken(
            Guid.NewGuid(),
            user.Id,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);
    }
}