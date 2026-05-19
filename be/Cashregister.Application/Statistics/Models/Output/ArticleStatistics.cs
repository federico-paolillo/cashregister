using System.Collections.Immutable;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents all-time article sales statistics and their totals.
/// </summary>
public sealed record ArticleStatistics(
    ImmutableArray<ArticleStatisticsItem> Items,
    ArticleStatisticsTotals Totals
);