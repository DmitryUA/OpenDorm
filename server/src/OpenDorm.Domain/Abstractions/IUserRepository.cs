using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default);
    Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}