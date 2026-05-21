using Cashregister.Domain;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents one raw order-item row for the sales statistics CSV export.
/// </summary>
public sealed record SalesStatisticsCsvRow(
    Identifier OrderId,
    OrderNumber OrderNumber,
    TimeStamp OrderDate,
    Identifier OrderItemId,
    Identifier ArticleId,
    string CurrentArticleDescription,
    string SoldDescription,
    bool ArticleRetired,
    long UnitPriceInCents,
    long Quantity,
    long? OrderTotalOverrideInCents
);