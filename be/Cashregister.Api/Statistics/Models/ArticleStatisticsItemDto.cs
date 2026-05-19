using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents one article statistics row in HTTP responses.
/// </summary>
public sealed record ArticleStatisticsItemDto(
    string ArticleId,
    string Description,
    long SoldUnits,
    long OrdersIncluded,
    long VolumeInCents
)
{
    public static ArticleStatisticsItemDto From(ArticleStatisticsItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ArticleStatisticsItemDto(
            item.ArticleId.Value,
            item.Description,
            item.SoldUnits,
            item.OrdersIncluded,
            item.VolumeInCents
        );
    }
}