using Cashregister.Application.Orders.Queries;
using Cashregister.Database.Entities;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchArticlesQuery(
    IApplicationDbContext dbContext,
    ArticleEntityMapper articleEntityMapper
) : IFetchArticlesQuery
{
    public async Task<Article[]> FetchAsync(params Identifier[] identifiers)
    {
        string[]? rawIdentifiers = identifiers
            .Select(x => x.Value)
            .ToArray();

        ArticleEntity[]? articleEntities = await dbContext.Articles
            .Where(a => rawIdentifiers.Contains(a.Id))
            .AsNoTracking()
            .ToArrayAsync();

        Article[]? articles = articleEntities
            .Select(articleEntityMapper.FromEntity)
            .ToArray();

        return articles;
    }
}