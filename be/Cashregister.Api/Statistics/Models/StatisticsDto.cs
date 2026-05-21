using System.Collections.Immutable;

using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents the complete statistics HTTP response.
/// </summary>
public sealed record StatisticsDto(
    ImmutableArray<ArticleInventoryItemDto> Articles,
    ImmutableArray<OrderStatisticsItemDto> Orders,
    OrderStatisticsSummaryDto Summary
)
{
    public static StatisticsDto From(StatisticsReport statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        return new StatisticsDto(
            statistics.Articles.Select(ArticleInventoryItemDto.From).ToImmutableArray(),
            statistics.Orders.Select(OrderStatisticsItemDto.From).ToImmutableArray(),
            OrderStatisticsSummaryDto.From(statistics.Summary)
        );
    }
}