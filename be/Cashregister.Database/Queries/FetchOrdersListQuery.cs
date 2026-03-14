using System.Linq.Expressions;

using Cashregister.Application.Orders.Models.Output;
using Cashregister.Database.Entities;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchOrdersListQuery(
    IApplicationDbContext applicationDbContext
) : PaginationQuery<OrderEntity, OrderListItem>
{
    protected override IQueryable<OrderEntity> GetQueryable()
    {
        return applicationDbContext.Orders.Include(o => o.Items);
    }

    protected override Expression<Func<OrderEntity, OrderListItem>> Projection =>
        o => new OrderListItem
        {
            Id = Identifier.From(o.Id),
            Number = OrderNumber.From(o.RowId),
            Date = TimeStamp.From(o.Date),
            Total = Cents.From(o.Items.Sum(i => i.Quantity * i.Price))
        };
}
