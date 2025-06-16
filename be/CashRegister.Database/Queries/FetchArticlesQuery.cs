using CashRegister.Application.Orders.Queries;
using CashRegister.Database.Entities;
using CashRegister.Database.Mappers;
using CashRegister.Domain;

using Microsoft.EntityFrameworkCore;

namespace CashRegister.Database.Queries;

public sealed class FetchArticlesQuery(
    IApplicationDbContext dbContext,
    ArticleEntityMapper articleEntityMapper
) : IFetchArticlesQuery
{
    public async Task<Article[]> FetchAsync(params Identifier[] identifiers)
    {
        var rawIdentifiers = identifiers
            .Select(x => x.Value)
            .ToArray();

        var articleEntities = await dbContext.Articles
            .Where(a => rawIdentifiers.Contains(a.Id))
            .AsNoTracking()
            .ToArrayAsync();

        var articles = articleEntities
            .Select(articleEntityMapper.FromEntity)
            .ToArray();

        return articles;
    }
}