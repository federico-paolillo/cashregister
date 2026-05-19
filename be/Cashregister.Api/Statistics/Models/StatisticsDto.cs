using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents the complete statistics HTTP response.
/// </summary>
public sealed record StatisticsDto(
    ArticleStatisticsDto Articles,
    OrderStatisticsSummaryDto Orders,
    OrderStatisticsSummaryDto OrdersTotals
)
{
    public static StatisticsDto From(OrderStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        return new StatisticsDto(
            ArticleStatisticsDto.From(statistics.Articles),
            OrderStatisticsSummaryDto.From(statistics.Orders),
            OrderStatisticsSummaryDto.From(statistics.OrdersTotals)
        );
    }
}