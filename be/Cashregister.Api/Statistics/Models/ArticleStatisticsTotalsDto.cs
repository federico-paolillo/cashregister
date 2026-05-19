using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents article statistics totals in HTTP responses.
/// </summary>
public sealed record ArticleStatisticsTotalsDto(
    long SoldUnits,
    long OrdersIncluded,
    long VolumeInCents
)
{
    public static ArticleStatisticsTotalsDto From(ArticleStatisticsTotals totals)
    {
        ArgumentNullException.ThrowIfNull(totals);

        return new ArticleStatisticsTotalsDto(
            totals.SoldUnits,
            totals.OrdersIncluded,
            totals.VolumeInCents
        );
    }
}