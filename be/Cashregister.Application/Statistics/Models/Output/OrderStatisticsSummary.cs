namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents all-time order-volume statistics.
/// </summary>
public sealed record OrderStatisticsSummary(
    long OrderCount,
    long ProducedArticles,
    long ExpectedVolumeInCents,
    long RealVolumeInCents,
    long DeltaInCents
);