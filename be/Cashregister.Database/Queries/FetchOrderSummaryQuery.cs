using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Queries;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchOrderSummaryQuery(
    IApplicationDbContext applicationDbContext
) : IFetchOrderSummaryQuery
{
    public async Task<OrderSummary?> FetchAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        string rawOrderId = orderId.Value;

        var rawOrderSummary = await applicationDbContext.Orders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(o => o.Id == rawOrderId)
            .Select(o => new
            {
                o.Id,
                o.Date,
                o.RowId,
                Total = o.Items.Sum(x => x.Price)
            })
            .SingleOrDefaultAsync();

        if (rawOrderSummary is null)
        {
            return null;
        }

        return new OrderSummary
        {
            Id = Identifier.From(rawOrderSummary.Id),
            Date = TimeStamp.From(rawOrderSummary.Date),
            Total = Cents.From(rawOrderSummary.Total),
            Number = OrderNumber.From(rawOrderSummary.RowId)
        };
    }
}