using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Search;
using OpenDorm.Infrastructure.Services;

namespace OpenDorm.Shared.Tests.Database;

public static class TestDbContextOptionsExtensions
{
    private static readonly byte[] HmacKey = Enumerable.Repeat((byte)1, 32).ToArray();

    public static IOccupantSearchIndex CreateOccupantSearchIndex() => new HmacOccupantSearchIndex(HmacKey);

    public static DbContextOptionsBuilder<TContext> UseOccupantSearchIndexForTests<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        IOccupantSearchIndex? searchIndex = null)
        where TContext : DbContext
    {
        return builder.AddInterceptors(
            new OccupantSearchIndexInterceptor(searchIndex ?? CreateOccupantSearchIndex()));
    }
}
