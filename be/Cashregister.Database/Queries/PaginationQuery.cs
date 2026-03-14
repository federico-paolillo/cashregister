using System.Collections.Immutable;
using System.Linq.Expressions;

using Cashregister.Application.Pagination;
using Cashregister.Database.Entities;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public abstract class PaginationQuery<TEntity, TListItem> : IPaginationQuery<TListItem>
    where TEntity : class, IIdentifiableEntity
    where TListItem : class, IPageItem
{
    protected abstract IQueryable<TEntity> GetQueryable();

    protected abstract Expression<Func<TEntity, TListItem>> Projection { get; }

    public async Task<ImmutableArray<TListItem>> FetchAsync(uint count, Identifier? after = null)
    {
        var integerCount = (int)count;
        var afterValue = after?.Value;

        var items = await GetQueryable()
            .Where(e => afterValue == null || e.Id.CompareTo(afterValue) > 0)
            .OrderBy(e => e.Id)
            .Take(integerCount)
            .Select(Projection)
            .ToArrayAsync();

        return [.. items];
    }

    public async Task<ImmutableArray<TListItem>> FetchUntilAsync(Identifier until)
    {
        ArgumentNullException.ThrowIfNull(until);

        var untilValue = until.Value;

        var items = await GetQueryable()
            .Where(e => e.Id.CompareTo(untilValue) <= 0)
            .OrderBy(e => e.Id)
            .Select(Projection)
            .ToArrayAsync();

        return [.. items];
    }
}
