using Cashregister.Domain;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents all-time sales statistics for one active article.
/// </summary>
public sealed record ArticleStatisticsItem(
    Identifier ArticleId,
    string Description,
    long SoldUnits,
    long OrdersIncluded,
    long VolumeInCents
);