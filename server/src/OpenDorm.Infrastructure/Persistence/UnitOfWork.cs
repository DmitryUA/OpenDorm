using OpenDorm.Application.Abstractions.Persistence;

namespace OpenDorm.Infrastructure.Persistence;

public class UnitOfWork(OpenDormDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}