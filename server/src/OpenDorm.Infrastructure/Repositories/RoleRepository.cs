using Microsoft.EntityFrameworkCore;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;

namespace OpenDorm.Infrastructure.Repositories;

public class RoleRepository(OpenDormDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.RolesDbSet.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Role?> GetByNameAsync(RoleName name, CancellationToken cancellationToken = default) =>
        await dbContext.RolesDbSet.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
}