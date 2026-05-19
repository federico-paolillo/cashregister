namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents the complete all-time statistics view.
/// </summary>
public sealed record OrderStatistics(
    ArticleStatistics Articles,
    OrderStatisticsSummary Orders,
    OrderStatisticsSummary OrdersTotals
);