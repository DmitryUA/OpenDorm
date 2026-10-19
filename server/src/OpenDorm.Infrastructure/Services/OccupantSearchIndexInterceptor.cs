using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenDorm.Application.Abstractions.Search;
using OpenDorm.Domain.Aggregates.Occupant;

namespace OpenDorm.Infrastructure.Services;

public sealed class OccupantSearchIndexInterceptor(IOccupantSearchIndex searchIndex) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateSearchIndexes(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateSearchIndexes(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void UpdateSearchIndexes(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<Occupant>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            entry.Property<byte[]?>("last_name_search_hash").CurrentValue =
                searchIndex.ComputeHash(entry.Entity.LastName.Value);
            entry.Property<byte[]?>("first_name_search_hash").CurrentValue =
                searchIndex.ComputeHash(entry.Entity.FirstName.Value);
            entry.Property<byte[]?>("patronymic_search_hash").CurrentValue =
                searchIndex.ComputeHash(entry.Entity.Patronymic?.Value);
        }
    }
}
