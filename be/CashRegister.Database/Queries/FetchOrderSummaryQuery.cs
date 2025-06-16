using CashRegister.Application.Orders.Models.Output;
using CashRegister.Application.Orders.Queries;
using CashRegister.Domain;

using Microsoft.EntityFrameworkCore;

namespace CashRegister.Database.Queries;

public sealed class FetchOrderSummaryQuery(
    IApplicationDbContext applicationDbContext
) : IFetchOrderSummaryQuery
{
    public async Task<OrderSummary?> FetchAsync(Identifier orderId)
    {
        var rawOrderId = orderId.Value;

        var rawOrderSummary = await applicationDbContext.Orders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(o => o.Id == rawOrderId)
            .Select(o => new
            {
                o.Id,
                o.Date,
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
            Total = Cents.From(rawOrderSummary.Total)
        };
    }
}