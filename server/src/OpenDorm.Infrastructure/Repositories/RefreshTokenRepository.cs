using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;

namespace OpenDorm.Infrastructure.Repositories;

public class RefreshTokenRepository(OpenDormDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokensDbSet.AddAsync(refreshToken, cancellationToken);

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokensDbSet
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        refreshToken.Revoke();
        return Task.CompletedTask;
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await dbContext.RefreshTokensDbSet
            .Where(rt => rt.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        dbContext.RefreshTokensDbSet.RemoveRange(expiredTokens);
    }
}