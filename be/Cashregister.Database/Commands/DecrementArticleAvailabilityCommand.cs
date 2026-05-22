using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Commands;

/// <summary>
/// Updates tracked article entities with soft availability decrements.
/// </summary>
public sealed class DecrementArticleAvailabilityCommand(
    IApplicationDbContext applicationDbContext
) : IDecrementArticleAvailabilityCommand
{
    public async Task DecrementAsync(IEnumerable<OrderRequestItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var quantityByArticleId = items
            .GroupBy(item => item.Article.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => (long)item.Quantity)
            );

        string[] articleIds = [.. quantityByArticleId.Keys];

        var articles = await applicationDbContext.Articles
            .Where(article => articleIds.Contains(article.Id) && article.QuantityAvailable != null)
            .ToArrayAsync();

        foreach (var article in articles)
        {
            article.QuantityAvailable -= quantityByArticleId[article.Id];
        }
    }
}