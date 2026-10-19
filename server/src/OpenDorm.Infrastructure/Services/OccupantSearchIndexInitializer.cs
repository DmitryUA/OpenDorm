using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Search;

namespace OpenDorm.Infrastructure.Services;

public sealed class OccupantSearchIndexInitializer(
    OpenDormDbContext dbContext,
    IOccupantSearchIndex searchIndex)
{
    private const int BatchSize = 500;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var occupants = await dbContext.OccupantsDbSet
                .Where(occupant =>
                    EF.Property<byte[]?>(occupant, "last_name_search_hash") == null ||
                    EF.Property<byte[]?>(occupant, "first_name_search_hash") == null)
                .OrderBy(occupant => occupant.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (occupants.Count == 0)
                return;

            foreach (var occupant in occupants)
            {
                var entry = dbContext.Entry(occupant);
                var lastNameHash = entry.Property<byte[]?>("last_name_search_hash");
                lastNameHash.CurrentValue = searchIndex.ComputeHash(occupant.LastName.Value);
                lastNameHash.IsModified = true;

                var firstNameHash = entry.Property<byte[]?>("first_name_search_hash");
                firstNameHash.CurrentValue = searchIndex.ComputeHash(occupant.FirstName.Value);
                firstNameHash.IsModified = true;

                var patronymicHash = entry.Property<byte[]?>("patronymic_search_hash");
                patronymicHash.CurrentValue = searchIndex.ComputeHash(occupant.Patronymic?.Value);
                patronymicHash.IsModified = true;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
    }
}
