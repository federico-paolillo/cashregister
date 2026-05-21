using System.Collections.Immutable;

using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

/// <summary>
/// Fetches all-time sales statistics from the application database.
/// </summary>
public sealed class FetchStatisticsQuery(
    IApplicationDbContext applicationDbContext
) : IFetchStatisticsQuery
{
    public async Task<StatisticsReport> FetchAsync(CancellationToken cancellationToken = default)
    {
        var articles = await FetchArticleInventoryAsync(cancellationToken);
        var orders = await FetchOrderStatisticsItemsAsync(cancellationToken);
        var salesRows = await FetchSalesStatisticsCsvRowsAsync(cancellationToken);
        var summary = new OrderStatisticsSummary(
            orders.Length,
            orders.Sum(x => x.ProducedArticles),
            orders.Sum(x => x.ExpectedVolumeInCents),
            orders.Sum(x => x.RealVolumeInCents),
            orders.Sum(x => x.DeltaInCents)
        );

        return new StatisticsReport(articles, orders, summary, salesRows);
    }

    private async Task<ImmutableArray<ArticleInventoryItem>> FetchArticleInventoryAsync(
        CancellationToken cancellationToken
    )
    {
        var sales = await applicationDbContext.OrderItems
            .AsNoTracking()
            .GroupBy(item => item.ArticleId)
            .Select(group => new
            {
                ArticleId = group.Key,
                SoldUnits = group.Sum(item => (long)item.Quantity)
            })
            .ToListAsync(cancellationToken);

        var articleIds = sales.Select(x => x.ArticleId).ToArray();

        var articles = await applicationDbContext.Articles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(article => articleIds.Contains(article.Id))
            .ToDictionaryAsync(article => article.Id, cancellationToken);

        return sales
            .OrderByDescending(x => x.SoldUnits)
            .ThenBy(x => articles[x.ArticleId].Description)
            .ThenBy(x => x.ArticleId)
            .Select(x => new ArticleInventoryItem(
                Identifier.From(x.ArticleId),
                articles[x.ArticleId].Description,
                articles[x.ArticleId].Retired,
                x.SoldUnits
            ))
            .ToImmutableArray();
    }

    private async Task<ImmutableArray<OrderStatisticsItem>> FetchOrderStatisticsItemsAsync(
        CancellationToken cancellationToken
    )
    {
        var rows = await applicationDbContext.Orders
            .AsNoTracking()
            .Select(order => new
            {
                order.Id,
                order.RowId,
                order.Date,
                ProducedArticles = applicationDbContext.OrderItems
                    .Where(item => item.OrderId == order.Id)
                    .Sum(item => (long?)item.Quantity) ?? 0L,
                ExpectedVolumeInCents = applicationDbContext.OrderItems
                    .Where(item => item.OrderId == order.Id)
                    .Sum(item => (long?)item.Quantity * item.Price) ?? 0L,
                order.TotalOverride
            })
            .OrderBy(x => x.RowId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x =>
            {
                var realVolumeInCents = x.TotalOverride ?? x.ExpectedVolumeInCents;

                return new OrderStatisticsItem(
                    Identifier.From(x.Id),
                    OrderNumber.From(x.RowId),
                    TimeStamp.From(x.Date),
                    x.ProducedArticles,
                    x.ExpectedVolumeInCents,
                    realVolumeInCents,
                    realVolumeInCents - x.ExpectedVolumeInCents,
                    x.TotalOverride is not null
                );
            })
            .ToImmutableArray();
    }

    private async Task<ImmutableArray<SalesStatisticsCsvRow>> FetchSalesStatisticsCsvRowsAsync(
        CancellationToken cancellationToken
    )
    {
        var rows = await (
                from item in applicationDbContext.OrderItems.AsNoTracking()
                join order in applicationDbContext.Orders.AsNoTracking()
                    on item.OrderId equals order.Id
                join article in applicationDbContext.Articles.IgnoreQueryFilters().AsNoTracking()
                    on item.ArticleId equals article.Id
                orderby order.RowId, item.Id
                select new
                {
                    OrderId = order.Id,
                    order.RowId,
                    order.Date,
                    OrderItemId = item.Id,
                    item.ArticleId,
                    CurrentArticleDescription = article.Description,
                    SoldDescription = item.Description,
                    ArticleRetired = article.Retired,
                    UnitPriceInCents = item.Price,
                    Quantity = (long)item.Quantity,
                    OrderTotalOverrideInCents = order.TotalOverride
                }
            )
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new SalesStatisticsCsvRow(
                Identifier.From(x.OrderId),
                OrderNumber.From(x.RowId),
                TimeStamp.From(x.Date),
                Identifier.From(x.OrderItemId),
                Identifier.From(x.ArticleId),
                x.CurrentArticleDescription,
                x.SoldDescription,
                x.ArticleRetired,
                x.UnitPriceInCents,
                x.Quantity,
                x.OrderTotalOverrideInCents
            ))
            .ToImmutableArray();
    }
}