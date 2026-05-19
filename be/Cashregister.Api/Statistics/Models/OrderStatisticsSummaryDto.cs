using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents order-volume statistics in HTTP responses.
/// </summary>
public sealed record OrderStatisticsSummaryDto(
    long OrderCount,
    long NominalVolumeInCents,
    long RealVolumeInCents,
    long DeltaInCents
)
{
    public static OrderStatisticsSummaryDto From(OrderStatisticsSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new OrderStatisticsSummaryDto(
            summary.OrderCount,
            summary.NominalVolumeInCents,
            summary.RealVolumeInCents,
            summary.DeltaInCents
        );
    }
}