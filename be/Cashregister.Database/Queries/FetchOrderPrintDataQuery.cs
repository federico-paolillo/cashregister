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
                    .Select(i => new
                    {
                        i.ArticleId,
                        i.Description,
                        i.Price,
                        i.Quantity
                    })
                    .ToList(),
                o.TotalOverride
            })
            .SingleOrDefaultAsync();

        if (projection is null)
        {
            return null;
        }

        string[] articleIds = [.. projection.Items.Select(i => i.ArticleId).Distinct()];

        var printDetailReceiptByArticle = await dbContext.Articles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => articleIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.PrintDetailReceipt);

        ImmutableArray<OrderPrintDataItem> items =
        [
            .. projection.Items.Select(i => new OrderPrintDataItem
            {
                Description = i.Description,
                Price = Cents.From(i.Price),
                Quantity = i.Quantity,
                PrintDetailReceipt = printDetailReceiptByArticle[i.ArticleId]
            })
        ];

        return new OrderPrintData
        {
            Id = Identifier.From(projection.Id),
            Number = OrderNumber.From(projection.RowId),
            Date = TimeStamp.From(projection.Date),
            Total = Cents.From(projection.TotalOverride ?? projection.Items.Sum(i => i.Price * i.Quantity)),
            Items = items
        };
    }
}