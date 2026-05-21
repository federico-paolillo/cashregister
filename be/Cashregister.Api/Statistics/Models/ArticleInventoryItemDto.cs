using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Api.Statistics.Models;

/// <summary>
/// Represents one article inventory row in HTTP responses.
/// </summary>
public sealed record ArticleInventoryItemDto(
    string ArticleId,
    string Description,
    bool Retired,
    long SoldUnits
)
{
    public static ArticleInventoryItemDto From(ArticleInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ArticleInventoryItemDto(
            item.ArticleId.Value,
            item.Description,
            item.Retired,
            item.SoldUnits
        );
    }
}