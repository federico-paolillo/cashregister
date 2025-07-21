using Cashregister.Application.Articles.Commands;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Commands;

public sealed class SaveArticleCommand(
    IApplicationDbContext applicationDbContext,
    ArticleEntityMapper articleEntityMapper
) : ISaveArticleCommand
{
    public async Task SaveAsync(Article newArticle)
    {
        var articleEntity = articleEntityMapper.ToEntity(newArticle);

        await applicationDbContext.Articles.AddAsync(articleEntity);
    }

    public async Task SaveAsync(RetiredArticle retiredArticle)
    {
        ArgumentNullException.ThrowIfNull(retiredArticle);

        var articleEntity = await applicationDbContext.Articles
            .FindAsync(retiredArticle.Id.Value);

        if (articleEntity is null)
        {
            return;
        }

        articleEntity.Retired = true;
    }
}