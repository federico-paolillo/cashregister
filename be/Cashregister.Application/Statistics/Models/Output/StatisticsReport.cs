using System.Collections.Immutable;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents the complete all-time statistics view and its raw CSV rows.
/// </summary>
public sealed record StatisticsReport(
    ImmutableArray<ArticleInventoryItem> Articles,
    ImmutableArray<OrderStatisticsItem> Orders,
    OrderStatisticsSummary Summary,
    ImmutableArray<SalesStatisticsCsvRow> SalesRows
);