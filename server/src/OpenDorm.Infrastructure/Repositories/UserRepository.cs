using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Repositories;

public class UserRepository(OpenDormDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.UsersDbSet
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default) =>
        await dbContext.UsersDbSet
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Login.Value == login.Value, cancellationToken);

    public async Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default) =>
        await dbContext.UsersDbSet
            .AnyAsync(u => u.Login.Value == login.Value, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.UsersDbSet.AddAsync(user, cancellationToken);
}