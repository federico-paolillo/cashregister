using Cashregister.Domain;

namespace Cashregister.Application.Statistics.Models.Output;

/// <summary>
/// Represents sold-unit inventory for one article.
/// </summary>
public sealed record ArticleInventoryItem(
    Identifier ArticleId,
    string Description,
    bool Retired,
    long SoldUnits
);