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
    public async Task<OrderStatistics> FetchAsync(CancellationToken cancellationToken = default)
    {
        var articleItems = await FetchArticleStatisticsItemsAsync(cancellationToken);
        var articleTotals = new ArticleStatisticsTotals(
            articleItems.Sum(x => x.SoldUnits),
            articleItems.Sum(x => x.OrdersIncluded),
            articleItems.Sum(x => x.VolumeInCents)
        );

        var orderStatistics = await FetchOrderStatisticsAsync(cancellationToken);

        return new OrderStatistics(
            new ArticleStatistics(articleItems, articleTotals),
            orderStatistics,
            orderStatistics
        );
    }

    private async Task<ImmutableArray<ArticleStatisticsItem>> FetchArticleStatisticsItemsAsync(
        CancellationToken cancellationToken
    )
    {
        var rows = await applicationDbContext.Articles
            .AsNoTracking()
            .GroupJoin(
                applicationDbContext.OrderItems.AsNoTracking(),
                article => article.Id,
                item => item.ArticleId,
                (article, items) => new
                {
                    ArticleId = article.Id,
                    article.Description,
                    SoldUnits = items.Sum(item => (long?)item.Quantity) ?? 0L,
                    OrdersIncluded = items.Select(item => item.OrderId).Distinct().Count(),
                    VolumeInCents = items.Sum(item => (long?)item.Quantity * item.Price) ?? 0L
                }
            )
            .OrderBy(x => x.ArticleId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new ArticleStatisticsItem(
                Identifier.From(x.ArticleId),
                x.Description,
                x.SoldUnits,
                x.OrdersIncluded,
                x.VolumeInCents
            ))
            .ToImmutableArray();
    }

    private async Task<OrderStatisticsSummary> FetchOrderStatisticsAsync(CancellationToken cancellationToken)
    {
        var rows = await applicationDbContext.Orders
            .AsNoTracking()
            .Select(order => new
            {
                NominalVolumeInCents = applicationDbContext.OrderItems
                    .Where(item => item.OrderId == order.Id)
                    .Sum(item => (long?)item.Quantity * item.Price) ?? 0L,
                order.TotalOverride
            })
            .ToListAsync(cancellationToken);

        var nominalVolumeInCents = rows.Sum(x => x.NominalVolumeInCents);
        var realVolumeInCents = rows.Sum(x => x.TotalOverride ?? x.NominalVolumeInCents);

        return new OrderStatisticsSummary(
            rows.Count,
            nominalVolumeInCents,
            realVolumeInCents,
            realVolumeInCents - nominalVolumeInCents
        );
    }
}