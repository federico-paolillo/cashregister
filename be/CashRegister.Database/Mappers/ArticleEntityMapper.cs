using CashRegister.Database.Entities;
using CashRegister.Domain;

namespace CashRegister.Database.Mappers;

public sealed class ArticleEntityMapper
{
    public Article FromEntity(ArticleEntity entity)
    {
        return new Article
        {
            Id = Identifier.From(entity.Id),
            Description = entity.Description,
            Price = Cents.From(entity.Price)
        };
    }
}