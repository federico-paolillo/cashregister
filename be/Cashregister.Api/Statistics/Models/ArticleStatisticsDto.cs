using System.Collections.Immutable;

using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents article sales statistics in HTTP responses.
/// </summary>
public sealed record ArticleStatisticsDto(
    ImmutableArray<ArticleStatisticsItemDto> Items,
    ArticleStatisticsTotalsDto Totals
)
{
    public static ArticleStatisticsDto From(ArticleStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        return new ArticleStatisticsDto(
            statistics.Items.Select(ArticleStatisticsItemDto.From).ToImmutableArray(),
            ArticleStatisticsTotalsDto.From(statistics.Totals)
        );
    }
}