using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Mappers;

public sealed class ArticleEntityMapper
{
    public Article FromEntity(ArticleEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new Article
        {
            Id = Identifier.From(entity.Id),
            Description = entity.Description,
            Price = Cents.From(entity.Price)
        };
    }
}