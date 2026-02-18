using System.Collections.Immutable;

using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchArticlesListQuery(
    IApplicationDbContext applicationDbContext
) : IFetchArticlesListQuery
{
    public async Task<ImmutableArray<ArticleListItem>> FetchAsync(uint count, Identifier? after = null)
    {
        var integerCount = (int)count;
        var afterValue = after?.Value;

        var articleListItems = await applicationDbContext.Articles
            .Where(a => afterValue == null || a.Id.CompareTo(afterValue) >= 0)
            .OrderBy(a => a.Id)
            .Take(integerCount)
            .Select(a => new ArticleListItem
            {
                Id = Identifier.From(a.Id),
                Description = a.Description,
                Price = Cents.From(a.Price)
            })
            .ToArrayAsync();

        return [.. articleListItems];
    }

    public async Task<ImmutableArray<ArticleListItem>> FetchUntilAsync(Identifier until)
    {
        ArgumentNullException.ThrowIfNull(until);

        var untilValue = until.Value;

        var articleListItems = await applicationDbContext.Articles
            .Where(a => a.Id.CompareTo(untilValue) < 0)
            .OrderBy(a => a.Id)
            .Select(a => new ArticleListItem
            {
                Id = Identifier.From(a.Id),
                Description = a.Description,
                Price = Cents.From(a.Price)
            })
            .ToArrayAsync();

        return [.. articleListItems];
    }
}