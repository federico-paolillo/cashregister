using Cashregister.Application.Articles.Queries;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchArticleQuery(
    IApplicationDbContext dbContext,
    ArticleEntityMapper articleEntityMapper
) : IFetchArticleQuery
{
    public async Task<Article?> FetchAsync(Identifier identifier)
    {
        var maybeArticleEntity = await dbContext.Articles
            .Where(a => a.Retired != false)
            .SingleOrDefaultAsync(x => x.Id == identifier.Value);

        if (maybeArticleEntity is null)
        {
            return null;
        }

        var article = articleEntityMapper.FromEntity(maybeArticleEntity);

        return article;
    }
}