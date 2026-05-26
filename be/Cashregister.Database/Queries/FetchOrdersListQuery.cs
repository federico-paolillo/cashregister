using System.Collections.Immutable;
using System.Linq.Expressions;

using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Database.Entities;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchOrdersListQuery(
    IApplicationDbContext applicationDbContext
) : IPaginationQuery<OrderListItem>
{
    public async Task<ImmutableArray<OrderListItem>> FetchAsync(uint count, Identifier? after = null)
    {
        var integerCount = (int)count;
        var afterValue = after?.Value;

        var items = await GetQueryable()
            .Where(e => afterValue == null || e.Id.CompareTo(afterValue) < 0)
            .OrderByDescending(e => e.Id)
            .Take(integerCount)
            .Select(GetProjection())
            .ToArrayAsync();

        return [.. items];
    }

    public async Task<ImmutableArray<OrderListItem>> FetchUntilAsync(Identifier until)
    {
        ArgumentNullException.ThrowIfNull(until);

        var untilValue = until.Value;

        var items = await GetQueryable()
            .Where(e => e.Id.CompareTo(untilValue) >= 0)
            .OrderByDescending(e => e.Id)
            .Select(GetProjection())
            .ToArrayAsync();

        return [.. items];
    }

    private IQueryable<OrderEntity> GetQueryable()
    {
        return applicationDbContext.Orders.Include(o => o.Items);
    }

    private static Expression<Func<OrderEntity, OrderListItem>> GetProjection()
    {
        return o => new OrderListItem
        {
            Id = Identifier.From(o.Id),
            Number = OrderNumber.From(o.RowId),
            Date = TimeStamp.From(o.Date),
            Total = Cents.From(o.Items.Sum(i => i.Quantity * i.Price)),
            TotalOverride = o.TotalOverride != null ? Cents.From(o.TotalOverride.Value) : null
        };
    }
}