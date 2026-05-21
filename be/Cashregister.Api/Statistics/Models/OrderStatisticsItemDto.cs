using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents one order statistics row in HTTP responses.
/// </summary>
public sealed record OrderStatisticsItemDto(
    string OrderId,
    string OrderNumber,
    long Date,
    long ProducedArticles,
    long ExpectedVolumeInCents,
    long RealVolumeInCents,
    long DeltaInCents,
    bool HasOverride
)
{
    public static OrderStatisticsItemDto From(OrderStatisticsItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new OrderStatisticsItemDto(
            item.OrderId.Value,
            item.OrderNumber.Value,
            item.Date.Value,
            item.ProducedArticles,
            item.ExpectedVolumeInCents,
            item.RealVolumeInCents,
            item.DeltaInCents,
            item.HasOverride
        );
    }
}