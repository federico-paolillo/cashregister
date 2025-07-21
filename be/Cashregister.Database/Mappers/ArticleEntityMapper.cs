using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Mappers;

public sealed class ArticleEntityMapper
{
    public Article FromEntity(ArticleEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Retired)
        {
            throw new InvalidOperationException("You are mapping a retired article to an article");
        }

        return new Article
        {
            Id = Identifier.From(entity.Id),
            Description = entity.Description,
            Price = Cents.From(entity.Price),
        };
    }

    public ArticleEntity ToEntity(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new ArticleEntity
        {
            Id = article.Id.Value,
            Description = article.Description,
            Price = article.Price.Value,
            Retired = false
        };
    }
}