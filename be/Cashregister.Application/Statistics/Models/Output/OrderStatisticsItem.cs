using Cashregister.Domain;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents produced article and volume statistics for one order.
/// </summary>
public sealed record OrderStatisticsItem(
    Identifier OrderId,
    OrderNumber OrderNumber,
    TimeStamp Date,
    long ProducedArticles,
    long ExpectedVolumeInCents,
    long RealVolumeInCents,
    long DeltaInCents,
    bool HasOverride
);