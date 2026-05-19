namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents totals for all-time article sales statistics.
/// </summary>
public sealed record ArticleStatisticsTotals(
    long SoldUnits,
    long OrdersIncluded,
    long VolumeInCents
);