using System.Collections.Immutable;

using Cashregister.Application.Receipts.Data;
using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

/// <summary>
///     Fetches receipt-specific order print data from the application database.
/// </summary>
public sealed class FetchOrderPrintDataQuery(
    IApplicationDbContext dbContext
) : IFetchOrderPrintDataQuery
{
    public async Task<OrderPrintData?> FetchAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        var projection = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId.Value)
            .Select(o => new
            {
                o.Id,
                o.RowId,
                o.Date,
                Items = o.Items
                    .OrderBy(i => i.Id)
                    .Select(i => new OrderPrintDataItem
                    {
                        Description = i.Description,
                        Quantity = i.Quantity
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();

        if (projection is null)
        {
            return null;
        }

        return new OrderPrintData
        {
            Id = Identifier.From(projection.Id),
            Number = OrderNumber.From(projection.RowId),
            Date = TimeStamp.From(projection.Date),
            Items = [.. projection.Items]
        };
    }
}