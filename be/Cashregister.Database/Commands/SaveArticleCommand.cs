using Cashregister.Application.Articles.Data;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

namespace Cashregister.Database.Commands;

public sealed class SaveArticleCommand(
    IApplicationDbContext applicationDbContext,
    ArticleEntityMapper articleEntityMapper
) : ISaveArticleCommand
{
    public async Task SaveAsync(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        var maybeArticleEntity = await applicationDbContext.Articles
            .FindAsync(article.Id.Value);

        if (maybeArticleEntity is null)
        {
            var articleEntity = articleEntityMapper.ToEntity(article);

            await applicationDbContext.Articles.AddAsync(articleEntity);
        }
        else
        {
            maybeArticleEntity.Description = article.Description;
            maybeArticleEntity.Price = article.Price.Value;
        }
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